using Kyoto.Exceptions;
using Kyoto.Models;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kyoto.Services.Implementations;

public class RestrictedUrlRepository(KyotoDbContext dbContext) : IRestrictedUrlRepository
{
    public async Task BlockUrl(string host, string path, string reason, DateTime? bannedUntil = null)
    {
        if (await dbContext.RestrictedUrls.AnyAsync(ru => ru.Host == host && ru.Path == path))
            throw new RestrictedUrlRepositoryException($"URL {host}{path} is already blocked");

        var restrictedUrl = new RestrictedUrl(host, path, reason, bannedUntil);

        await dbContext.RestrictedUrls.AddAsync(restrictedUrl);
        await dbContext.SaveChangesAsync();
    }

    public async Task UnblockUrl(string host, string path)
    {
        var restrictedUrl =
            await dbContext.RestrictedUrls.FirstOrDefaultAsync(ru => ru.Host == host && ru.Path == path);

        if (restrictedUrl == null) throw new RestrictedUrlRepositoryException($"URL {host}{path} is not blocked");

        dbContext.RestrictedUrls.Remove(restrictedUrl);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsUrlBlocked(string host, string path)
    {
        var restrictedUrl = await dbContext.RestrictedUrls
            .Where(ru => ru.Host == "*" || ru.Host == host)
            .Where(ru => ru.Path == "*" || ru.Path == path)
            .FirstOrDefaultAsync();

        if (restrictedUrl == null) return false;

        if (!restrictedUrl.BannedUntil.HasValue) return true;

        var isBanned = restrictedUrl.BannedUntil.Value > DateTime.UtcNow;

        if (!isBanned)
        {
            dbContext.RestrictedUrls.Remove(restrictedUrl);
            await dbContext.SaveChangesAsync();
        }

        return isBanned;
    }
}