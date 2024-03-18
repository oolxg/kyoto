using Kyoto.Exceptions;
using Kyoto.Models;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kyoto.Services.Implementations;

public class UserRequestRepository(KyotoDbContext context) : IUserRequestRepository
{
    private KyotoDbContext Context => context;

    public async Task SaveUserRequestAsync(UserRequest userRequest)
    {
        if (await FindUserRequestAsync(userRequest.Id) != null) return;

        await Context.UserRequests.AddAsync(userRequest);
        await Context.SaveChangesAsync();
    }

    public async Task<UserRequest?> FindUserRequestAsync(Guid requestId)
    {
        return await Context.UserRequests.FindAsync(requestId);
    }

    public async Task<List<UserRequest>> FindUserRequestsByTokenAsync(string token)
    {
        return await Context.UserRequests
            .Include(ur => ur.TokenInfo)
            .Include(ur => ur.IpInfo)
            .Where(ur => ur.TokenInfo != null && ur.TokenInfo!.Token == token)
            .OrderBy(ur => ur.RequestDate)
            .ToListAsync();
    }

    public async Task<List<UserRequest>> FindUserRequestsByIpAsync(string ipToFind)
    {
        return await Context.UserRequests
            .Include(ur => ur.TokenInfo)
            .Include(ur => ur.IpInfo)
            .Where(ur => ur.IpInfo.Ip == ipToFind)
            .OrderBy(ur => ur.RequestDate)
            .ToListAsync();
    }

    public async Task<List<UserRequest>> GetUserRequestsOnEndPointsAsync(string host, string path, DateTime start)
    {
        return await Context.UserRequests
            .Where(ur =>
                (host == "*" || ur.Host == host) &&
                (path == "*" || ur.Path == path) &&
                ur.RequestDate >= start
            )
            .OrderByDescending(ur => ur.RequestDate)
            .ToListAsync();

    }

    public async Task<List<UserRequest>> GetBlockedRequestsAsync(string host, string path, DateTime? start = null)
    {
        start ??= DateTime.MinValue;
        return await Context.UserRequests
            .Where(ur =>
                (host == "*" || ur.Host == host) &&
                (path == "*" || ur.Path == path) &&
                ur.RequestDate >= start &&
                ur.IsBlocked
            )
            .OrderByDescending(ur => ur.RequestDate)
            .ToListAsync();
    }

    public async Task UpdateUserRequestAsync(UserRequest userRequest)
    {
        if (await FindUserRequestAsync(userRequest.Id) == null)
            throw new UserRequestRepositoryException("User request with given id does not exist");
        Context.UserRequests.Update(userRequest);
        await Context.SaveChangesAsync();
    }
}