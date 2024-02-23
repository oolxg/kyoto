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
}