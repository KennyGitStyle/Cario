using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionServiceDbContext _dbContext;

    public AuctionFinishedConsumer(AuctionServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("--> AuctionFinished Consuming...");
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if(context.Message.IsItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice ?
            Status.Finished : Status.ReserveNotMet;

        await _dbContext.SaveChangesAsync();

    }
}
