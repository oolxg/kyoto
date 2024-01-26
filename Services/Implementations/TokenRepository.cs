using Microsoft.EntityFrameworkCore;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;
using Smug.Utils;

namespace Smug.Services.Implementations;

public class TokenRepository : ITokenRepository
{
    private readonly SmugDbContext _context;

    public TokenRepository(SmugDbContext context)
    {
        _context = context;
    }
    
    public async Task<TokenInfo> SaveTokenAsync(string token)
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

    public async Task<TokenInfo> BanTokenAsync(string token, string? reason = null)
    {
        var bannedToken = await FindTokenAsync(token) ?? new TokenInfo(token);
        bannedToken.UpdateStatus(TokenInfo.TokenStatus.Banned, reason);

        await _context.Tokens.AddAsync(bannedToken);
        await _context.SaveChangesAsync();
        return bannedToken;
    }

    public async Task UnbanTokenAsync(string token)
    {
        var bannedToken = await FindTokenAsync(token);
        
        if (bannedToken == null)
        {
            throw new TokenRepositoryException("TokenInfo is not banned");
        }
        
        _context.Tokens.Remove(bannedToken);
        await _context.SaveChangesAsync();
    }
    
    public async Task<TokenInfo?> FindTokenAsync(string token)
    {
        return await _context.Tokens.FirstOrDefaultAsync(bt => bt.Token == token);;
    }

    public Task<TokenInfo?> FindTokenAsync(Guid id)
    {
        return _context.Tokens.FirstOrDefaultAsync(bt => bt.Id == id);
    }
}