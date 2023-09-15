using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auction: ");

        stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

        while(!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000);
        }
    }

    private async ValueTask CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(x => x.AuctionEnd <= DateTime.UtcNow)
            .Match(x => !x.Finished)
            .ExecuteAsync(stoppingToken)
            .ConfigureAwait(false);

        if (finishedAuctions.Count == 0)
        {
            return;
        }

        _logger.LogInformation("==> Found {count} auctions that have completed", finishedAuctions.Count);

        using var scope = _services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Update all auctions to 'Finished' in one go (assuming your database supports batch updates)
        var updateTasks = finishedAuctions.Select(auction => {
            auction.Finished = true;
            return auction.SaveAsync(null, stoppingToken);
        });
        await Task.WhenAll(updateTasks).ConfigureAwait(false);

        // Find the winning bids in parallel
        var findWinningBidTasks = finishedAuctions.Select(auction => 
            DB.Find<Bid>()
                .Match(a => a.AuctionId == auction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Descending(s => s.Amount))
                .ExecuteFirstAsync(stoppingToken)
        );
        var winningBids = await Task.WhenAll(findWinningBidTasks).ConfigureAwait(false);

        // Publish auction finished events
        var publishTasks = finishedAuctions.Zip(winningBids, (auction, winningBid) => 
            endpoint.Publish(new AuctionFinished 
            {
                IsItemSold = winningBid is not null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = auction.Seller
            }, stoppingToken)
        );
        await Task.WhenAll(publishTasks).ConfigureAwait(false);
    }

}
