using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionServiceDbContext _dbContext;

    public BidPlacedConsumer(AuctionServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> BidPlaced Consuming...");

        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if(auction.CurrentHighBid is null 
         || context.Message.BidStstus.Contains("Accepted")
         && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _dbContext.SaveChangesAsync();
        }


    }
}
