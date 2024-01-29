using Microsoft.EntityFrameworkCore;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;
using Smug.Exceptions;

namespace Smug.Services.Implementations;

public class IpRepository : IIpRepository
{
    private readonly SmugDbContext _dbContext;

    public IpRepository(SmugDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IpAddressInfo> SaveIpIfNeededAsync(string ipToSave)
    {
        var ip = await FindIpAsync(ipToSave);
        if (ip != null)
        {
            return ip;
        }
        
        ip = new IpAddressInfo(ipToSave);
        await _dbContext.Ips.AddAsync(ip);
        await _dbContext.SaveChangesAsync();
        
        return ip;
    }

    public async Task<IpAddressInfo> BanIpAsync(string ip, string reason)
    {
        return await BanIpAsync(ip, false, reason);
    }
    
    public async Task<IpAddressInfo> BanIpAsync(string ip, bool shouldHide, string reason)
    {
        var bannedIp = await FindIpAsync(ip);
        
        if (bannedIp == null)
        {
            bannedIp = new IpAddressInfo(ip);
            await _dbContext.Ips.AddAsync(bannedIp);
        }
        
        if (bannedIp.Status == IpAddressInfo.IpStatus.Banned)
        {
            throw new IpRepositoryException("IpAddressInfo is already banned");
        }
        
        bannedIp.UpdateStatus(IpAddressInfo.IpStatus.Banned, reason);
        bannedIp.ShouldHideIfBanned = shouldHide;
        
        await _dbContext.SaveChangesAsync();
        return bannedIp;
    }
    
    public async Task UnbanIpAsync(string ip, string reason)
    {
        var bannedIp = await FindIpAsync(ip);
        
        if (bannedIp == null)
        {
            throw new IpRepositoryException("IpAddressInfo is not in the database");
        }
        
        bannedIp.UpdateStatus(IpAddressInfo.IpStatus.Normal, reason);
    }

    public async Task<IpAddressInfo?> FindIpAsync(string ipToFind)
    {
        return await _dbContext.Ips.FirstOrDefaultAsync(ipInfo => ipInfo.Ip == ipToFind);
    }

    public async Task<IpAddressInfo?> FindIpAsync(Guid id)
    {
        return await _dbContext.Ips.FindAsync(id);
    }
    
    public async Task WhitelistIpAsync(string ip, string reason)
    {
        var ipToWhitelist = await FindIpAsync(ip);
        
        if (ipToWhitelist == null)
        {
            ipToWhitelist = new IpAddressInfo(ip);
            await _dbContext.Ips.AddAsync(ipToWhitelist);
        }
        
        if (ipToWhitelist.Status == IpAddressInfo.IpStatus.Whitelisted)
        {
            throw new IpRepositoryException("IpAddressInfo is already whitelisted");
        }
        
        ipToWhitelist.UpdateStatus(IpAddressInfo.IpStatus.Whitelisted, reason);
        
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task ChangeShouldHideIfBannedAsync(string ip, bool shouldHide)
    {
        var ipToChange = await FindIpAsync(ip);
        
        if (ipToChange == null)
        {
            throw new IpRepositoryException("IpAddressInfo is not in the database");
        }
        
        ipToChange.ShouldHideIfBanned = shouldHide;
        
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task AddIpAddressesAsync(string ip, List<Guid> tokenIds)
    {
        var ipAddressInfo = await FindIpAsync(ip);
        if (ipAddressInfo == null)
        {
            throw new IpRepositoryException("TokenInfo is not in the database");
        }
        
        foreach (var tokenId in tokenIds)
        {
            ipAddressInfo.IpTokens.Add(new IpToken
            {
                IpId = ipAddressInfo.Id,
                TokenId = tokenId
            });
        }
        
        await _dbContext.SaveChangesAsync();
    }
}