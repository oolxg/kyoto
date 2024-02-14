using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Smug.Models.SmugDbContext;

public class SmugDbContext : DbContext
{
    public SmugDbContext(DbContextOptions<SmugDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserRequest>()
            .Property(u => u.Headers)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, Formatting.None),
                v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v)!
                );
        
        modelBuilder.Entity<IpToken>().HasKey(it => new { it.IpAddressInfoId, it.TokenInfoId });
    }

    public virtual DbSet<RestrictedUrl> RestrictedUrls { get; set; } = null!;
    public virtual DbSet<IpToken> IpTokens { get; set; } = null!;
    public virtual DbSet<TokenInfo> Tokens { get; set; } = null!;
    public virtual DbSet<IpAddressInfo> Ips { get; set; } = null!;
    public virtual DbSet<UserRequest> UserRequests { get; set; } = null!;
}