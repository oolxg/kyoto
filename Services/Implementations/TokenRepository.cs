using Microsoft.EntityFrameworkCore;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;
using Smug.Exceptions;

namespace Smug.Services.Implementations;

public class TokenRepository : ITokenRepository
{
    private readonly SmugDbContext _context;

    public TokenRepository(SmugDbContext context)
    {
        _context = context;
    }
    
    public async Task<TokenInfo> SaveTokenIfNeededAsync(string token)
    {
        var tokenInfo = await FindTokenAsync(token);
        if (tokenInfo != null)
        {
            return tokenInfo;
        }
        
        tokenInfo = new TokenInfo(token);
        await _context.Tokens.AddAsync(tokenInfo);
        await _context.SaveChangesAsync();
        return tokenInfo;
    }

    public async Task<TokenInfo> BanTokenAsync(string token, string reason)
    {
        var bannedToken = await FindTokenAsync(token);
        
        if (bannedToken == null)
        {
            bannedToken = new TokenInfo(token);
            await _context.Tokens.AddAsync(bannedToken);    
        }
        
        bannedToken.UpdateStatus(TokenInfo.TokenStatus.Banned, reason);
        
        await _context.SaveChangesAsync();
        return bannedToken;
    }

    public async Task UnbanTokenAsync(string token, string? reason = null)
    {
        var bannedToken = await FindTokenAsync(token);
        
        if (bannedToken == null)
        {
            throw new TokenRepositoryException("TokenInfo is not in the database");
        }
        
        bannedToken.UpdateStatus(TokenInfo.TokenStatus.Normal, reason);
        await _context.SaveChangesAsync();
    }
    
    public async Task<TokenInfo?> FindTokenAsync(string token)
    {
        return await _context.Tokens.FirstOrDefaultAsync(bt => bt.Token == token);;
    }

    public async Task<TokenInfo?> FindTokenAsync(Guid id)
    {
        return await _context.Tokens.FindAsync(id);
    }
    
    public async Task AddIpAddressesAsync(string token, List<Guid> ipAddressIds)
    {
        var tokenInfo = await FindTokenAsync(token);
        if (tokenInfo == null)
        {
            throw new TokenRepositoryException("TokenInfo is not in the database");
        }
        
        foreach (var ipAddress in ipAddressIds)
        {
            tokenInfo.IpTokens.Add(new IpToken
            {
                IpId = ipAddress,
                TokenId = tokenInfo.Id
            });
        }
        
        await _context.SaveChangesAsync();
    }
}