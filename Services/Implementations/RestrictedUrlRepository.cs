using Microsoft.EntityFrameworkCore;
using Smug.Exceptions;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;

namespace Smug.Services.Implementations;

public class RestrictedUrlRepository(SmugDbContext dbContext) : IRestrictedUrlRepository
{
    public async Task BlockUrl(string host, string path, string reason, DateTime? bannedUntil = null)
    {
        if (await dbContext.RestrictedUrls.AnyAsync(ru => ru.Host == host && ru.Path == path))
        {
            throw new RestrictedUrlRepositoryException($"URL {host}{path} is already blocked");
        }
        
        var restrictedUrl = new RestrictedUrl(host, path, reason, bannedUntil);
        
        await dbContext.RestrictedUrls.AddAsync(restrictedUrl);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task UnblockUrl(string host, string path)
    {
        var restrictedUrl = await dbContext.RestrictedUrls.FirstOrDefaultAsync(ru => ru.Host == host && ru.Path == path);
        
        if (restrictedUrl == null)
        {
            throw new RestrictedUrlRepositoryException($"URL {host}{path} is not blocked");
        }
        
        dbContext.RestrictedUrls.Remove(restrictedUrl);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task<bool> IsUrlBlocked(string host, string path)
    {
        var restrictedUrl = await dbContext.RestrictedUrls
            .Where(ru => ru.Host == "*" || ru.Host == host)
            .Where(ru => ru.Path == "*" || ru.Path == path)
            .FirstOrDefaultAsync();
        
        if (restrictedUrl == null)
        {
            return false;
        }
        
        if (!restrictedUrl.BannedUntil.HasValue)
        {
            return true;
        }
        
        var isBanned = restrictedUrl.BannedUntil.Value > DateTime.UtcNow;
        
        if (!isBanned)
        {
            dbContext.RestrictedUrls.Remove(restrictedUrl);
            await dbContext.SaveChangesAsync();
        }
        
        return isBanned;
    }
}