using Kyoto.Exceptions;
using Kyoto.Models;

namespace Kyoto.Services.Interfaces;

public interface ITokenRepository
{
    /// <summary>
    /// Save token if needed in database
    /// </summary>
    /// <param name="token">Token to save</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the newly created <see cref="TokenInfo"/> object.</returns>
    public Task<TokenInfo> FindOrCreateTokenAsync(string token);

    /// <summary>
    /// Save token if needed in database and ban it
    /// </summary>
    /// <param name="token">Token to ban</param>
    /// <param name="reason">Reason for ban</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the banned <see cref="TokenInfo"/> object.</returns>
    public Task<TokenInfo> BanTokenAsync(string token, string reason);

    /// <summary>
    /// Unban token
    /// </summary>
    /// <param name="token">Token to unban</param>
    /// <param name="reason">Reason for unban</param>
    /// <throws><see cref="TokenRepositoryException"/> if the token is not banned.</throws>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task UnbanTokenAsync(string token, string reason);
    
    /// <summary>
    /// Whitelist token
    /// </summary>
    /// <param name="token">Token to whitelist</param>
    /// <param name="reason">Reason for whitelist</param>
    /// <throws><see cref="TokenRepositoryException"/> if the token is not found.</throws>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task WhitelistTokenAsync(string token, string reason);

    /// <summary>
    /// Find token by token string
    /// </summary>
    /// <param name="id">Token entity id</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="TokenInfo"/> object, or null if not found.</returns>
    public Task<TokenInfo?> FindTokenAsync(Guid id);

    /// <summary>
    /// Find token by id
    /// </summary>
    /// <param name="token">Token string</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="TokenInfo"/> object, or null if not found.</returns>
    public Task<TokenInfo?> FindTokenAsync(string token);

    /// <summary>
    /// Add user request associated with token
    /// </summary>
    /// <param name="token">Token string</param>
    /// <param name="userRequestId">User request id</param>
    /// <throws><see cref="TokenRepositoryException"/> If the token or user request is not found.</throws>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddUserRequestToTokenAsync(string token, Guid userRequestId);
    
    /// <summary>
    /// Find user requests associated with token
    /// </summary>
    /// <param name="token">Token string</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of <see cref="UserRequest"/> objects associated with the token.</returns>
    public Task<List<IpAddressInfo>> FindIpsByTokenAsync(string token);
}