using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly AuctionServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _endpoint;

    public AuctionController(AuctionServiceDbContext context, IMapper mapper, IPublishEndpoint endpoint)
    {
        _context = context;
        _mapper = mapper;
        _endpoint = endpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item!.Make).AsQueryable();

        if(!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.LastUpdatedAt
                .CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        var result = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if(auction is null)
            return NotFound("Auction not found.");

        var auctionDto = _mapper.Map<AuctionDto>(auction);
        
        return Ok(auctionDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        // TODO: add current user as seller

        auction.Seller = "test";

        await _context.Auctions.AddAsync(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _endpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if(!result)
        {
            return BadRequest("Could not save changes to the DB");
        }

        return CreatedAtAction(nameof(GetAuctionById), 
            new {auction.Id}, newAuction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

        if(auction is null)
        {
            return NotFound("Auction cannot be found.");
        }

        // TODO: check seller == username

        auction.Item!.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item!.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item!.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await _endpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;

        if(result) return Ok();

        return BadRequest("Problem saving changes");

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if(auction is null)
        {
            return NotFound("Auction not found.");
        }

        // TODO: check seller == username.

        _context.Auctions.Remove(auction);

        await _endpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _context.SaveChangesAsync() > 0;
        
        if(!result) 
        {
            return BadRequest("Auction is invalid.");
        }

        return Ok();
    }
}
