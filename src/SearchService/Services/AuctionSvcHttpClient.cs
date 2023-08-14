﻿using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public AuctionSvcHttpClient(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.LastUpdatedAt))
            .Project(x => x.LastUpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _client.GetFromJsonAsync<List<Item>>
            (_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
    }
}
