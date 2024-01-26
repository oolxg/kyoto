using System.Net;
using Microsoft.EntityFrameworkCore;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;
using Smug.Utils;

namespace Smug.Services.Implementations;

public class IpRepository : IIpRepository
{
    private readonly SmugDbContext _dbContext;

    public IpRepository(SmugDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IpAddressInfo> SaveIpAsync(string ipToSave)
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

    public async Task<IpAddressInfo> BanIpAsync(string ip, string? reason = null)
    {
        return await BanIpAsync(ip, false, reason);
    }
    
    public async Task<IpAddressInfo> BanIpAsync(string ip, bool shouldHide, string? reason = null)
    {
        var bannedIp = await FindIpAsync(ip);
        
        if (bannedIp == null)
        {
            bannedIp = new IpAddressInfo(ip);
            await _dbContext.Ips.AddAsync(bannedIp);
        }
        
        bannedIp.UpdateStatus(IpAddressInfo.IpStatus.Banned, reason);
        bannedIp.ShouldHideIfBanned = shouldHide;
        
        await _dbContext.SaveChangesAsync();
        return bannedIp;
    }
    
    public async Task UnbanIpAsync(string ip, string? reason = null)
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
        return await _dbContext.Ips.FirstOrDefaultAsync(ip => ip.Id == id);
    }
    
    public async Task WhitelistIpAsync(string ip, string? reason = null)
    {
        var ipToWhitelist = await FindIpAsync(ip);
        
        if (ipToWhitelist == null)
        {
            ipToWhitelist = new IpAddressInfo(ip);
            await _dbContext.Ips.AddAsync(ipToWhitelist);
        }
        
        ipToWhitelist.UpdateStatus(IpAddressInfo.IpStatus.Whitelisted, reason);
        
        await _dbContext.SaveChangesAsync();
    }
}