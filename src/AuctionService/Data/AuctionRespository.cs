﻿using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService;

public class AuctionRespository : IAuctionRespository
{
    private readonly AuctionServiceDbContext _context;
    private readonly IMapper _mapper;

    public AuctionRespository(AuctionServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task AddAuction(Auction auction) => await _context.Auctions.AddAsync(auction);

    public async Task<AuctionDto> GetAuction(Guid id) => 
        await _context.Auctions
            .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    

    public async Task<Auction> GetAuctionEntity(Guid id)
    {
        return await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AuctionDto>> GetAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.LastUpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveAuction(Auction auction) => _context.Auctions.Remove(auction);

    public async Task<bool> SaveChanges() => await _context.SaveChangesAsync() > 0;
}
