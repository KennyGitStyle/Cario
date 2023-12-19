using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _appFactory;
    private readonly HttpClient _httpClient;
    private ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory appFactory)
    {
        _appFactory = appFactory;
        _httpClient = appFactory.CreateClient();
        _testHarness = appFactory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        // arange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());

    }
    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _appFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionServiceDbContext>();
        DbHelper.ReinitDbForTest(db);
        return Task.CompletedTask;
    }

    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "test",
            Color = "test",
            Mileage = 10,
            Year = 2003,
            ReservePrice = 300
        };
    }
}
