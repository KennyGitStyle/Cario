﻿using System.Security.Claims;

namespace AuctionService.IntegrationTests.Util;

public class AuthHelper
{
    public static Dictionary<string, object> GetBearerForUser(string username) => new() { { ClaimTypes.Name, username } };
}
