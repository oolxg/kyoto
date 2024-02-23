using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Smug.Models.SmugDbContext;

namespace Tests.Helpers;

public class DbContextFactory
{
    public static SmugDbContext CreateDbContext()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .Build();

        var options = new DbContextOptionsBuilder<SmugDbContext>()
            .UseNpgsql(config.GetConnectionString("DefaultConnection"))
            .Options;

        var dbContext = new SmugDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
        return dbContext;
    }

    public static void DisposeDbContext(SmugDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }
}