using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Kyoto.Models.KyotoDbContext;

namespace Tests.Helpers;

public class DbContextFactory
{
    public static KyotoDbContext CreateDbContext()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .Build();

        var options = new DbContextOptionsBuilder<KyotoDbContext>()
            .UseNpgsql(config.GetConnectionString("DefaultConnection"))
            .Options;

        var dbContext = new KyotoDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
        return dbContext;
    }

    public static void DisposeDbContext(KyotoDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }
}