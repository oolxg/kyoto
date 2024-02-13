using Microsoft.EntityFrameworkCore;
using Smug.Exceptions;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;

namespace Smug.Services.Implementations;

public class RestrictedUrlRepository : IRestrictedUrlRepository
{
    private readonly SmugDbContext _dbContext;

    public RestrictedUrlRepository(SmugDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BlockUrl(string host, string path, string reason, DateTime? bannedUntil = null)
    {
        if (await IsUrlBlocked(host, path))
        {
            throw new RestrictedUrlRepositoryException($"URL {host}{path} is already blocked");
        }
        
        var restrictedUrl = new RestrictedUrl(host, path, reason, bannedUntil);
        
        await _dbContext.RestrictedUrls.AddAsync(restrictedUrl);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task UnblockUrl(string host, string path)
    {
        var restrictedUrl = await _dbContext.RestrictedUrls.FirstOrDefaultAsync(ru => ru.Host == host && ru.Path == path);
        
        if (restrictedUrl == null)
        {
            throw new RestrictedUrlRepositoryException($"URL {host}{path} is not blocked");
        }
        
        _dbContext.RestrictedUrls.Remove(restrictedUrl);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<bool> IsUrlBlocked(string host, string path)
    {
        var restrictedUrl = await _dbContext.RestrictedUrls
            .FirstOrDefaultAsync(ru => ru.Host == host && ru.Path == path);
        
        if (restrictedUrl == null)
        {
            return false;
        }
        
        if (!restrictedUrl.BannedUntil.HasValue)
        {
            return true;
        }
        
        return restrictedUrl.BannedUntil.Value > DateTime.UtcNow;
    }
}