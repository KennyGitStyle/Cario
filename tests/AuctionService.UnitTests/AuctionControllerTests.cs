using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRespository> _mockAuctionRepo;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _auctionsController;
    private readonly IMapper _mapper;
    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _mockAuctionRepo = new Mock<IAuctionRespository>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc => {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);
        _auctionsController =
             new AuctionsController(_mockAuctionRepo.Object, _mapper, _mockPublishEndpoint.Object)
             {
                ControllerContext = new() 
                {
                    HttpContext = new DefaultHttpContext{User = Helper.GetClaimsPrincipal()}
                }
             };
    }

    [Fact]
    public async Task GetAuctionById_WithNoParams_Returns10Auctions()
    {
        // arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _mockAuctionRepo.Setup(repo => repo.GetAuctions(null)).ReturnsAsync(auctions);

        // act
        var result = await _auctionsController.GetAllAuctions(null);

        // assert
        Assert.Equal(10, result.Value?.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();
        _mockAuctionRepo.Setup(repo => repo.GetAuction(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _auctionsController.GetAuctionById(auction.Id);

        // assert
        Assert.Equal(auction.Make, result.Value?.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        // arrange
        _mockAuctionRepo.Setup(repo => repo.GetAuction(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // act
        var result = await _auctionsController.GetAuctionById(Guid.NewGuid());

        // assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtActionResult()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _mockAuctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _mockAuctionRepo.Setup(repo => repo.SaveChanges()).ReturnsAsync(true);

        // act
        var result = await _auctionsController.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;

        // assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // arrange
        var auctionDto = _fixture.Create<CreateAuctionDto>();
        _mockAuctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _mockAuctionRepo.Setup(repo => repo.SaveChanges()).ReturnsAsync(false);

        // act
        var result = await _auctionsController.CreateAuction(auctionDto);

        // assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _mockAuctionRepo.Setup(repo => repo.GetAuctionEntity(It.IsAny<Guid>())).ReturnsAsync(auction);
        _mockAuctionRepo.Setup(repo => repo.SaveChanges()).ReturnsAsync(true);

        // act
        var result = await _auctionsController.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "no-test";
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _mockAuctionRepo.Setup(repo => repo.GetAuctionEntity(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _auctionsController.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _mockAuctionRepo.Setup(repo => repo.GetAuctionEntity(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _auctionsController.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
         // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "test";

        _mockAuctionRepo.Setup(repo => repo.GetAuctionEntity(It.IsAny<Guid>())).ReturnsAsync(auction);
        _mockAuctionRepo.Setup(repo => repo.SaveChanges()).ReturnsAsync(true);

        // act
        var result = await _auctionsController.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<OkResult>(result);

    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        _mockAuctionRepo.Setup(repo => repo.GetAuctionEntity(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _auctionsController.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "no-test";
        _mockAuctionRepo.Setup(repo => repo.GetAuctionEntity(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _auctionsController.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<ForbidResult>(result);
    }
}
