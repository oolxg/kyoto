using Microsoft.EntityFrameworkCore;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;

namespace Smug.Services.Implementations;

public class UserRequestRepository(SmugDbContext context) : IUserRequestRepository
{
    private SmugDbContext Context => context;

    public async Task SaveUserRequestAsync(UserRequest userRequest)
    {
        if (await FindUserRequestAsync(userRequest.Id) != null)
        {
            return;
        }
        
        await Context.UserRequests.AddAsync(userRequest);
        await Context.SaveChangesAsync();
    }
    
    public async Task<UserRequest?> FindUserRequestAsync(Guid requestId)
    {
        return await Context.UserRequests.FindAsync(requestId);
    }

    public async Task<List<UserRequest>> FindUserRequestByTokenAsync(string token)
    {
        return await Context.UserRequests
            .Include(ur => ur.TokenInfo)
            .Include(ur => ur.IpInfo)
            .Where(ur => ur.TokenInfo != null && ur.TokenInfo!.Token == token)
            .ToListAsync();
    }

    public async Task<List<UserRequest>> FindUserRequestByIpAsync(string ipToFind)
    {
        return await Context.UserRequests
            .Include(ur => ur.TokenInfo)
            .Include(ur => ur.IpInfo)
            .Where(ur => ur.IpInfo.Ip == ipToFind)
            .ToListAsync();
    }
}