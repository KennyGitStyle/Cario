using AuctionService.Data;
using AuctionService.Entities;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;

namespace AuctionService.Services;

public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionServiceDbContext _dbContext;
    private readonly ILogger<GrpcAuctionService> _logger;
    private readonly IMemoryCache _cache;

    public GrpcAuctionService(AuctionServiceDbContext dbContext, 
                                  ILogger<GrpcAuctionService> logger,
                                  IMemoryCache cache)
    {
            _dbContext = dbContext;
            _logger = logger;
            _cache = cache;
    }

    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, 
                                                                    ServerCallContext context)
    {
        _logger.LogInformation("Received gRPC request for auction with ID {AuctionId}", request.Id);

        if (string.IsNullOrEmpty(request.Id))
        {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Argument"));
        }


        // Try to get the auction from the cache
        if (!_cache.TryGetValue(request.Id, out Auction auction))
        {
            auction = await _dbContext.Auctions
                                      .FindAsync(Guid.Parse(request.Id))
                                      .ConfigureAwait(false);

            if (auction == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            _cache.Set(request.Id, auction, cacheEntryOptions);
        }

        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel
            {
                AuctionEnd = auction.AuctionEnd.ToString(),
                Id = auction.Id.ToString(),
                ReservePrice = auction.ReservePrice,
                Seller = auction.Seller
            }
        };

        return response;
    }
}

