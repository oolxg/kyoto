using Smug.Exceptions;
using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Tests.Fakes;

public class IpRepositoryFake
    (IUserRequestRepository userRequestRepository, ITokenRepository tokenRepository) : IIpRepository
{
    public List<IpAddressInfo> Ips { get; private set; } = new();
    public List<IpToken> IpTokens { get; set; } = new();
    public int FindOrCreateIpAsyncCount { get; private set; } = 0;
    public int BanIpIfNeededAsync2ParamsCount { get; private set; } = 0;
    public int BanIpIfNeededAsync3ParamsCount { get; private set; } = 0;
    public int UnbanIpAsyncCount { get; private set; } = 0;
    public int FindIpAsyncByIpCount { get; private set; } = 0;
    public int FindIpAsyncByIdCount { get; private set; } = 0;
    public int WhitelistIpAsyncCount { get; private set; } = 0;
    public int ChangeShouldHideIfBannedAsyncCount { get; private set; } = 0;
    public int AddTokenAsyncIfNeededCount { get; private set; } = 0;
    public int AddUserRequestToIpAsyncCount { get; private set; } = 0;

    public Task<IpAddressInfo> FindOrCreateIpAsync(string ipToSave)
    {
        FindOrCreateIpAsyncCount++;
        var ip = Ips.FirstOrDefault(i => i.Ip == ipToSave);
        if (ip != null) return Task.FromResult(ip);

        ip = new IpAddressInfo(ipToSave);
        Ips.Add(ip);
        return Task.FromResult(ip);
    }

    public Task<IpAddressInfo> BanIpIfNeededAsync(string ip, string reason)
    {
        BanIpIfNeededAsync2ParamsCount++;
        return BanIpIfNeededAsync(ip, false, reason);
    }

    public Task<IpAddressInfo> BanIpIfNeededAsync(string ip, bool shouldHide, string reason)
    {
        BanIpIfNeededAsync3ParamsCount++;
        var bannedIp = Ips.FirstOrDefault(i => i.Ip == ip);

        if (bannedIp == null)
        {
            bannedIp = new IpAddressInfo(ip);
            Ips.Add(bannedIp);
        }

        if (bannedIp.Status == IpStatus.Banned) return Task.FromResult(bannedIp);

        bannedIp.UpdateStatus(IpStatus.Banned, reason);
        bannedIp.ShouldHideIfBanned = shouldHide;

        return Task.FromResult(bannedIp);
    }

    public Task UnbanIpAsync(string ip, string reason)
    {
        UnbanIpAsyncCount++;
        var bannedIp = Ips.FirstOrDefault(i => i.Ip == ip);

        if (bannedIp == null) throw new IpRepositoryException("Ip not found");

        bannedIp.UpdateStatus(IpStatus.Normal, reason);
        return Task.CompletedTask;
    }

    public Task<IpAddressInfo?> FindIpAsync(string ipToFind)
    {
        FindIpAsyncByIpCount++;
        return Task.FromResult(Ips.FirstOrDefault(i => i.Ip == ipToFind));
    }

    public Task<IpAddressInfo?> FindIpAsync(Guid id)
    {
        FindIpAsyncByIdCount++;
        return Task.FromResult(Ips.FirstOrDefault(i => i.Id == id));
    }

    public Task WhitelistIpAsync(string ip, string reason)
    {
        WhitelistIpAsyncCount++;
        var ipInfo = Ips.FirstOrDefault(i => i.Ip == ip);
        if (ipInfo == null) throw new IpRepositoryException("Ip not found");

        if (ipInfo.Status == IpStatus.Whitelisted) throw new IpRepositoryException("Ip already whitelisted");

        ipInfo.UpdateStatus(IpStatus.Whitelisted, reason);
        return Task.CompletedTask;
    }

    public Task ChangeShouldHideIfBannedAsync(string ip, bool shouldHide)
    {
        ChangeShouldHideIfBannedAsyncCount++;
        var ipInfo = Ips.FirstOrDefault(i => i.Ip == ip);
        if (ipInfo == null) throw new IpRepositoryException("Ip not found");

        ipInfo.ShouldHideIfBanned = shouldHide;
        return Task.CompletedTask;
    }

    public Task AddTokenAsyncIfNeeded(string ip, Guid tokenId)
    {
        AddTokenAsyncIfNeededCount++;
        var ipInfo = Ips.FirstOrDefault(i => i.Ip == ip);
        if (ipInfo == null) throw new IpRepositoryException("Ip not found");

        var token = tokenRepository.FindTokenAsync(tokenId).Result;
        if (token == null) throw new IpRepositoryException("Token not found");

        var ipToken = new IpToken
        {
            IpAddressInfoId = ipInfo.Id,
            TokenInfoId = token.Id
        };
        IpTokens.Add(ipToken);
        return Task.CompletedTask;
    }

    public Task AddUserRequestToIpAsync(string ip, Guid userRequestId)
    {
        AddUserRequestToIpAsyncCount++;
        var ipInfo = Ips.FirstOrDefault(i => i.Ip == ip);
        if (ipInfo == null) throw new IpRepositoryException("Ip not found");

        var userRequest = userRequestRepository.FindUserRequestAsync(userRequestId).Result;
        if (userRequest == null) throw new IpRepositoryException("User request not found");

        ipInfo.UserRequests.Add(userRequest);
        return Task.CompletedTask;
    }
}