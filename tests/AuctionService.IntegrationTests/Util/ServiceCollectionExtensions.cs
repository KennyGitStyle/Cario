using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Fixtures;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuctionServiceDbContext>));

        if (descriptor is not null)
        { 
            services.Remove(descriptor);
        }
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AuctionServiceDbContext>();

            db.Database.Migrate();
            DbHelper.InitDbForTest(db);
    }
}
