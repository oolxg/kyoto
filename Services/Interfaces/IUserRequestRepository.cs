using Kyoto.Exceptions;
using Kyoto.Models;

namespace Kyoto.Services.Interfaces;

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
    Task<List<UserRequest>> FindUserRequestsByTokenAsync(string token);

    /// <summary>
    /// Find user requests by ip
    /// </summary>
    /// <param name="ip">User request ip</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of found <see cref="UserRequest"/> objects.</returns>
    Task<List<UserRequest>> FindUserRequestsByIpAsync(string ip);

    /// <summary>
    /// Get user requests on endpoints
    /// </summary>
    /// <param name="host">Host of the endpoint</param>
    /// <param name="path">Path of the endpoint</param>
    /// <param name="start">Start date</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of found <see cref="UserRequest"/> objects.</returns>
    Task<List<UserRequest>> GetUserRequestsOnEndPointsAsync(string host, string path, DateTime start);

    /// <summary>
    /// Get last blocked request for given endpoint
    /// </summary>
    /// <param name="host">Host of the endpoint</param>
    /// <param name="path">Path of the endpoint</param>
    /// <param name="start">Start date</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of found <see cref="UserRequest"/> objects.</returns>
    Task<List<UserRequest>> GetBlockedRequestsAsync(string host, string path, DateTime? start = null);

    /// <summary>
    /// Updates user request
    /// </summary>
    /// <param name="userRequest">User request to update</param>
    /// <throws><see cref="UserRequestRepositoryException"/> if user request is not found</throws>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateUserRequestAsync(UserRequest userRequest);
}