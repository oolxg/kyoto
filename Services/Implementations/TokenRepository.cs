using Kyoto.Exceptions;
using Kyoto.Models;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kyoto.Services.Implementations;

public class TokenRepository(KyotoDbContext context, IUserRequestRepository userRequestRepository) : ITokenRepository
{
    public async Task<TokenInfo> FindOrCreateTokenAsync(string token)
    {
        var tokenInfo = await FindTokenAsync(token);
        if (tokenInfo != null) return tokenInfo;

        tokenInfo = new TokenInfo(token);
        await context.Tokens.AddAsync(tokenInfo);
        await context.SaveChangesAsync();
        return tokenInfo;
    }

    public async Task<TokenInfo> BanTokenAsync(string token, string reason)
    {
        var bannedToken = await FindTokenAsync(token);

        if (bannedToken == null)
        {
            bannedToken = new TokenInfo(token);
            await context.Tokens.AddAsync(bannedToken);
        }

        bannedToken.UpdateStatus(TokenStatus.Banned, reason);

        await context.SaveChangesAsync();
        return bannedToken;
    }

    public async Task UnbanTokenAsync(string token, string reason)
    {
        var bannedToken = await FindTokenAsync(token);

        if (bannedToken == null)
        {
            bannedToken = new TokenInfo(token);
            await context.Tokens.AddAsync(bannedToken);
        }

        if (bannedToken.Status != TokenStatus.Banned) throw new TokenRepositoryException("TokenInfo is not banned");

        bannedToken.UpdateStatus(TokenStatus.Normal, reason);
        await context.SaveChangesAsync();
    }

    public async Task<TokenInfo?> FindTokenAsync(string token)
    {
        return await context.Tokens.FirstOrDefaultAsync(bt => bt.Token == token);
        ;
    }

    public async Task<TokenInfo?> FindTokenAsync(Guid id)
    {
        return await context.Tokens.FindAsync(id);
    }

    public async Task AddIpAddressIfNeededAsync(string token, Guid ipAddressId)
    {
        var tokenInfo = await FindTokenAsync(token);
        if (tokenInfo == null) throw new TokenRepositoryException("TokenInfo is not in the database");

        if (await context.Ips.FindAsync(ipAddressId) == null)
            throw new TokenRepositoryException("IpAddress is not in the database");

        var pivot = new IpToken(ipAddressId, tokenInfo.Id);

        if (await context.IpTokens.ContainsAsync(pivot) == false)
        {
            tokenInfo.IpTokens.Add(pivot);
            await context.SaveChangesAsync();
        }
    }

    public async Task AddUserRequestToTokenAsync(string token, Guid userRequestId)
    {
        var tokenInfo = await FindTokenAsync(token);
        if (tokenInfo == null) throw new TokenRepositoryException("TokenInfo is not in the database");

        var userRequest = await userRequestRepository.FindUserRequestAsync(userRequestId);
        if (userRequest == null) throw new TokenRepositoryException("UserRequest is not in the database");

        userRequest.TokenInfo = tokenInfo;

        tokenInfo.UserRequests.Add(userRequest);

        await context.SaveChangesAsync();
    }
}