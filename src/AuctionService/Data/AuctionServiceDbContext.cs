using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionServiceDbContext : DbContext
{
    public DbSet<Auction> Auctions { get; set; }
    public AuctionServiceDbContext(DbContextOptions options) : base(options)
    {
    }
}
