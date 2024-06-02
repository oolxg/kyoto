using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kyoto.Models;
using Kyoto.Services.Interfaces;

namespace Kyoto.Tests.Fakes;

public class UserRequestRepositoryFake : IUserRequestRepository
{
    public List<UserRequest> UserRequests { get; } = new();
    public int SaveUserRequestAsyncCount { get; private set; } = 0;
    public int FindUserRequestAsyncCount { get; private set; } = 0;
    public int FindUserRequestByTokenAsyncCount { get; private set; } = 0;
    public int FindUserRequestByIpAsyncCount { get; private set; } = 0;
    public int GetRequestsAsyncCount { get; private set; } = 0;
    public int UpdateUserRequestAsyncCount { get; private set; } = 0;

    public async Task SaveUserRequestAsync(UserRequest userRequest)
    {
        UserRequests.Add(userRequest);
        SaveUserRequestAsyncCount++;

        await Task.CompletedTask;
    }

    public Task<UserRequest?> FindUserRequestAsync(Guid requestId)
    {
        FindUserRequestAsyncCount++;
        return Task.FromResult(UserRequests.FirstOrDefault(ur => ur.Id == requestId));
    }

    public Task<List<UserRequest>> FindUserRequestsByTokenAsync(string token)
    {
        FindUserRequestByTokenAsyncCount++;
        return Task.FromResult(UserRequests.Where(ur => ur.TokenInfo != null && ur.TokenInfo!.Token == token).ToList());
    }

    public Task<List<UserRequest>> FindUserRequestsByIpAsync(string ipToFind)
    {
        FindUserRequestByIpAsyncCount++;
        return Task.FromResult(UserRequests.Where(ur => ur.IpInfo.Ip == ipToFind).ToList());
    }

    public Task<List<UserRequest>> GetRequestsAsync(string host, string path, bool includeNonBlocked = false, DateTime? start = null, DateTime? end = null)
    {
        GetRequestsAsyncCount++;
        start ??= DateTime.MinValue;
        end ??= DateTime.MaxValue;
        return Task.FromResult(UserRequests
            .Where(ur =>
                    (host == "*" || ur.Host == host) &&
                    (path == "*" || ur.Path == path) &&
                    ur.RequestDate >= start && ur.RequestDate <= end &&
                    (includeNonBlocked || ur.IsBlocked)
                )
            .OrderByDescending(ur => ur.RequestDate)
            .ToList());
    }

    public Task UpdateUserRequestAsync(UserRequest userRequest)
    {
        UpdateUserRequestAsyncCount++;
        return Task.CompletedTask;
    }
}