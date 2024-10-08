using Kyoto.Exceptions;
using Kyoto.Models;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kyoto.Services.Implementations;

public class IpRepository : IIpRepository
{
    private readonly KyotoDbContext _dbContext;
    private readonly IUserRequestRepository _userRequestRepository;

    public IpRepository(KyotoDbContext dbContext, IUserRequestRepository userRequestRepository)
    {
        _dbContext = dbContext;
        _userRequestRepository = userRequestRepository;
    }

    public async Task<IpAddressInfo> FindOrCreateIpAsync(string ipToSave)
    {
        var ip = await FindIpAsync(ipToSave);
        if (ip != null) return ip;

        ip = new IpAddressInfo(ipToSave);
        await _dbContext.Ips.AddAsync(ip);
        await _dbContext.SaveChangesAsync();

        return ip;
    }

    public async Task<IpAddressInfo> BanIpIfNeededAsync(string ip, string reason)
    {
        var bannedIp = await FindIpAsync(ip);

        if (bannedIp == null)
        {
            bannedIp = new IpAddressInfo(ip);
            await _dbContext.Ips.AddAsync(bannedIp);
        }

        if (bannedIp.Status == IpStatus.Banned) return bannedIp;

        bannedIp.UpdateStatus(IpStatus.Banned, reason);

        await _dbContext.SaveChangesAsync();
        return bannedIp;
    }

    public async Task UnbanIpAsync(string ip, string reason)
    {
        var bannedIp = await FindIpAsync(ip);

        if (bannedIp == null) throw new IpRepositoryException("IpAddressInfo is not in the database");

        bannedIp.UpdateStatus(IpStatus.Normal, reason);
        await _dbContext.SaveChangesAsync();
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

        if (ipToWhitelist.Status == IpStatus.Whitelisted)
            throw new IpRepositoryException("IpAddressInfo is already whitelisted");

        ipToWhitelist.UpdateStatus(IpStatus.Whitelisted, reason);

        await _dbContext.SaveChangesAsync();
    }

    public async Task ChangeShouldHideIfBannedAsync(string ip, bool shouldHide)
    {
        var ipToChange = await FindIpAsync(ip);

        if (ipToChange == null)
        {
            ipToChange = new IpAddressInfo(ip);
            await _dbContext.Ips.AddAsync(ipToChange);
        }

        ipToChange.ShouldHideIfBanned = shouldHide;

        await _dbContext.SaveChangesAsync();
    }

    public async Task AddTokenAsyncIfNeeded(string ip, Guid tokenId)
    {
        var ipAddressInfo = await FindIpAsync(ip);
        if (ipAddressInfo == null) throw new IpRepositoryException("Token is not in the database");

        if (await _dbContext.Tokens.FindAsync(tokenId) == null)
            throw new IpRepositoryException("Token is not in the database");

        var pivot = new IpToken(ipAddressInfo.Id, tokenId);

        if (await _dbContext.IpTokens.ContainsAsync(pivot) == false)
        {
            ipAddressInfo.IpTokens.Add(pivot);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task AddUserRequestToIpAsync(string ip, Guid userRequestId)
    {
        var ipAddressInfo = await FindIpAsync(ip);
        if (ipAddressInfo == null) throw new IpRepositoryException("IP is not in the database");

        var userRequest = await _userRequestRepository.FindUserRequestAsync(userRequestId);
        if (userRequest == null) throw new IpRepositoryException("UserRequest is not in the database");

        userRequest.IpInfo = ipAddressInfo;

        ipAddressInfo.UserRequests.Add(userRequest);

        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<List<TokenInfo>> FindTokensByIpAsync(string ip)
    {
        var ipAddressInfo = await FindIpAsync(ip);
        if (ipAddressInfo == null) throw new IpRepositoryException("IP is not in the database");

        await _dbContext.Entry(ipAddressInfo).Collection(ipInfo => ipInfo.IpTokens).LoadAsync();
        
        var tokens = new List<TokenInfo>();
        foreach (var ipToken in ipAddressInfo.IpTokens)
        {
            var token = await _dbContext.Tokens.FindAsync(ipToken.TokenInfoId);
            if (token == null) throw new IpRepositoryException("Token is not in the database");
            tokens.Add(token);
        }
        
        return tokens;
    }
}