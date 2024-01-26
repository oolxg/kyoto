using Smug.Models;

namespace Smug.Services.Interfaces;

public interface IUserRequestRepository
{
    Task SaveUserRequestAsync(UserRequest userRequest);   
    Task<UserRequest?> FindUserRequestAsync(Guid id);
    Task<List<UserRequest>> FindUserRequestByTokenAsync(string token);
    Task<List<UserRequest>> FindUserRequestByIpAsync(string ip);
}