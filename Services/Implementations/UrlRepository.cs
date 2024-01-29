using Microsoft.EntityFrameworkCore;
using Smug.Exceptions;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;

namespace Smug.Services.Implementations;

public class UrlRepository : IUrlRepository
{
    private readonly SmugDbContext _dbContext;

    public UrlRepository(SmugDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<RestrictedUrl> RestrictUrlAsync(string host, string path, string reason)
    {
        var url = await FindRestrictedUrlAsync(host, path);
        if (url != null)
        {
            return url;
        }
        
        url = new RestrictedUrl(host, path, reason);
        await _dbContext.RestrictedUrls.AddAsync(url);
        await _dbContext.SaveChangesAsync();
        
        return url;
    }

    public async Task RemoveRestrictionAsync(string host, string path)
    {
        var url = await FindRestrictedUrlAsync(host, path);
        if (url == null)
        {
            throw new UrlRepositoryException("Url is not restricted");
        }
        
        _dbContext.RestrictedUrls.Remove(url);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<RestrictedUrl?> FindRestrictedUrlAsync(string host, string path)
    {
        return await _dbContext.RestrictedUrls
            .Where(url => url.Host == host && url.Path == path)
            .FirstOrDefaultAsync();
    }
}