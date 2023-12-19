using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRespository
{
    Task<List<AuctionDto>> GetAuctions(string date);
    Task<AuctionDto> GetAuction(Guid id);
    Task<Auction> GetAuctionEntity(Guid id);
    Task AddAuction(Auction auction);
    void RemoveAuction(Auction auction);
    Task<bool> SaveChanges();

}
