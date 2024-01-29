using Smug.Models;

namespace Smug.Services.Interfaces;

public interface IUserRequestRepository
{
    /// <summary>
    /// Save user request in database
    /// </summary>
    /// <param name="userRequest">User request to save</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SaveUserRequestAsync(UserRequest userRequest);   
    /// <summary>
    /// Find user request by id
    /// </summary>
    /// <param name="id">User request id</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="UserRequest"/> object, or null if not found.</returns>
    Task<UserRequest?> FindUserRequestAsync(Guid id);
    /// <summary>
    /// Find user requests by token
    /// </summary>
    /// <param name="token">User request token</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of found <see cref="UserRequest"/> objects.</returns>
    Task<List<UserRequest>> FindUserRequestByTokenAsync(string token);
    /// <summary>
    /// Find user requests by ip
    /// </summary>
    /// <param name="ip">User request ip</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of found <see cref="UserRequest"/> objects.</returns>
    Task<List<UserRequest>> FindUserRequestByIpAsync(string ip);
}