using Microsoft.EntityFrameworkCore;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Interfaces;

namespace Smug.Services.Implementations;

public class UserRequestRepository : IUserRequestRepository
{
    private readonly SmugDbContext _context;

    public UserRequestRepository(SmugDbContext context)
    {
        _context = context;
    }

    public async Task SaveUserRequestAsync(UserRequest userRequest)
    {
        if (await FindUserRequestAsync(userRequest.Id) != null)
        {
            return;
        }
        
        await _context.UserRequests.AddAsync(userRequest);
        await _context.SaveChangesAsync();
    }
    
    public async Task<UserRequest?> FindUserRequestAsync(Guid requestId)
    {
        return await _context.UserRequests.FindAsync(requestId);
    }

    public async Task<List<UserRequest>> FindUserRequestByTokenAsync(string token)
    {
        return await _context.UserRequests.Where(ur => ur.Token == token).ToListAsync();
    }

    public async Task<List<UserRequest>> FindUserRequestByIpAsync(string ipToFind)
    {
        return await _context.UserRequests.Where(ur => ur.IpAddress == ipToFind).ToListAsync();
    }
}