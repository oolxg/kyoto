using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kyoto.Exceptions;
using Kyoto.Models;
using Kyoto.Services.Interfaces;

public class TokenRepositoryFake(IUserRequestRepository userRequestRepository) : ITokenRepository
{
    public List<TokenInfo> Tokens { get; private set; } = new();
    public List<IpAddressInfo> Ips { get; set; } = new();
    public List<IpToken> IpTokens { get; set; } = new();
    public int FindOrCreateTokenAsyncCount { get; private set; } = 0;
    public int BanTokenAsyncCount { get; private set; } = 0;
    public int UnbanTokenAsyncCount { get; private set; } = 0;
    public int FindTokenAsyncByTokenCount { get; private set; } = 0;
    public int FindTokenAsyncByIdCount { get; private set; } = 0;
    public int AddIpAddressIfNeededAsyncCount { get; private set; } = 0;
    public int AddUserRequestToTokenAsyncCount { get; private set; } = 0;
    public int WhiteListTokenAsyncCount { get; private set; } = 0;
    public int FindIpsByTokenAsyncCount { get; private set; } = 0;

    public Task<TokenInfo> FindOrCreateTokenAsync(string token)
    {
        FindOrCreateTokenAsyncCount++;
        var tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
        if (tokenInfo != null) return Task.FromResult(tokenInfo);

        tokenInfo = new TokenInfo(token);
        Tokens.Add(tokenInfo);
        return Task.FromResult(tokenInfo);
    }

    public Task<TokenInfo> BanTokenAsync(string token, string reason)
    {
        BanTokenAsyncCount++;
        var tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
        if (tokenInfo == null)
        {
            tokenInfo = new TokenInfo(token);
            Tokens.Add(tokenInfo);
        }

        if (tokenInfo.Status == TokenStatus.Banned) return Task.FromResult(tokenInfo);
        tokenInfo.UpdateStatus(TokenStatus.Banned, reason);
        return Task.FromResult(tokenInfo);
    }

    public Task UnbanTokenAsync(string token, string reason)
    {
        UnbanTokenAsyncCount++;
        var tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
        if (tokenInfo == null)
        {
            tokenInfo = new TokenInfo(token);
            Tokens.Add(tokenInfo);
        }

        if (tokenInfo.Status != TokenStatus.Banned) throw new TokenRepositoryException("TokenInfo is not banned");

        tokenInfo.UpdateStatus(TokenStatus.Normal, reason);
        return Task.CompletedTask;
    }

    public Task<TokenInfo?> FindTokenAsync(string token)
    {
        FindTokenAsyncByTokenCount++;
        return Task.FromResult(Tokens.FirstOrDefault(t => t.Token == token));
    }

    public Task<TokenInfo?> FindTokenAsync(Guid id)
    {
        FindTokenAsyncByIdCount++;
        return Task.FromResult(Tokens.FirstOrDefault(t => t.Id == id));
    }

    public Task AddIpAddressIfNeededAsync(string token, Guid ipAddressId)
    {
        AddIpAddressIfNeededAsyncCount++;
        var tokenInfo = FindTokenAsync(token).Result;
        if (tokenInfo == null) throw new TokenRepositoryException("TokenInfo is not in the database");

        if (Ips.FirstOrDefault(i => i.Id == ipAddressId) == null)
            throw new TokenRepositoryException("IpAddress is not in the database");

        var pivot = new IpToken(ipAddressId, tokenInfo.Id);

        if (IpTokens.Contains(pivot) == false) tokenInfo.IpTokens.Add(pivot);

        return Task.CompletedTask;
    }

    public Task AddUserRequestToTokenAsync(string token, Guid userRequestId)
    {
        AddUserRequestToTokenAsyncCount++;
        var tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
        if (tokenInfo == null) throw new TokenRepositoryException("TokenInfo is not in the database");

        var userRequest = userRequestRepository.FindUserRequestAsync(userRequestId).Result;
        if (userRequest == null) throw new TokenRepositoryException("UserRequest is not in the database");

        userRequest.TokenInfo = tokenInfo;

        tokenInfo.UserRequests.Add(userRequest);

        return Task.CompletedTask;
    }
    
    public Task WhitelistTokenAsync(string token, string reason)
    {
        WhiteListTokenAsyncCount++;
        var tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
        if (tokenInfo == null)
        {
            throw new TokenRepositoryException("TokenInfo is not in the database");
        }

        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, reason);
        return Task.CompletedTask;
    }
    
    public Task<List<IpAddressInfo>> FindIpsByTokenAsync(string token)
    {
        FindIpsByTokenAsyncCount++;
        var tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
        if (tokenInfo == null) throw new TokenRepositoryException("TokenInfo is not in the database");

        var ips = new List<IpAddressInfo>();
        foreach (var ipToken in IpTokens.Where(it => it.TokenInfoId == tokenInfo.Id))
        {
            var ip = Ips.FirstOrDefault(i => i.Id == ipToken.IpAddressInfoId);
            if (ip == null) throw new TokenRepositoryException("IpAddress is not in the database");
            ips.Add(ip);
        }

        return Task.FromResult(ips);
    }
}