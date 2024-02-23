using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Tests.Fakes;

public class UserRequestRepositoryFake : IUserRequestRepository
{
    public List<UserRequest> UserRequests { get; } = new();
    public int SaveUserRequestAsyncCount { get; private set; } = 0;
    public int FindUserRequestAsyncCount { get; private set; } = 0;
    public int FindUserRequestByTokenAsyncCount { get; private set; } = 0;
    public int FindUserRequestByIpAsyncCount { get; private set; } = 0;
    public int GetUserRequestsOnEndPointsAsyncCount { get; private set; } = 0;
    public int GetBlockedRequestsAsyncCount { get; private set; } = 0;
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

    public Task<List<UserRequest>> FindUserRequestByTokenAsync(string token)
    {
        FindUserRequestByTokenAsyncCount++;
        return Task.FromResult(UserRequests.Where(ur => ur.TokenInfo != null && ur.TokenInfo!.Token == token).ToList());
    }

    public Task<List<UserRequest>> FindUserRequestByIpAsync(string ipToFind)
    {
        FindUserRequestByIpAsyncCount++;
        return Task.FromResult(UserRequests.Where(ur => ur.IpInfo.Ip == ipToFind).ToList());
    }

    public Task<List<UserRequest>> GetUserRequestsOnEndPointsAsync(string host, string path, DateTime start)
    {
        GetUserRequestsOnEndPointsAsyncCount++;
        return Task.FromResult(UserRequests.Where(ur => ur.Host == host && ur.Path == path && ur.RequestDate >= start)
            .ToList());
    }

    public Task<List<UserRequest>> GetBlockedRequestsAsync(string host, string path, DateTime? start = null)
    {
        GetBlockedRequestsAsyncCount++;
        start ??= DateTime.MinValue;
        return Task.FromResult(UserRequests.Where(ur => ur.Host == host && ur.Path == path && ur.RequestDate >= start)
            .ToList());
    }

    public Task UpdateUserRequestAsync(UserRequest userRequest)
    {
        UpdateUserRequestAsyncCount++;
        return Task.CompletedTask;
    }
}