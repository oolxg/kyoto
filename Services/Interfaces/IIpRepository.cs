using Kyoto.Exceptions;
using Kyoto.Models;

namespace Kyoto.Services.Interfaces;

public interface IIpRepository
{
    /// <summary>
    /// Saves the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to be saved.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the newly created <see cref="IpAddressInfo"/> object.</returns>
    public Task<IpAddressInfo> FindOrCreateIpAsync(string ip);

    /// <summary>
    /// Bans the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to be banned.</param>
    /// <param name="reason">The reason for banning the IP address.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the banned <see cref="IpAddressInfo"/> object.</returns>
    public Task<IpAddressInfo> BanIpIfNeededAsync(string ip, string reason);

    /// <summary>
    /// Unbans the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to be unbanned.</param>
    /// <param name="reason">The reason for unbanning the IP address.</param>
    /// <throws><see cref="IpRepositoryException"/> if the IP address is not banned.</throws>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task UnbanIpAsync(string ip, string reason);

    /// <summary>
    /// Finds and retrieves the banned IP information based on the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to search for.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="IpAddressInfo"/> object, or null if not found.</returns>
    public Task<IpAddressInfo?> FindIpAsync(string ip);
    
    /// <summary>
    /// Finds and retrieves the banned IP information based on the specified IP address asynchronously.
    /// </summary>
    /// <param name="id">The IP address id to search for.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="IpAddressInfo"/> object, or null if not found.</returns>
    public Task<IpAddressInfo?> FindIpAsync(Guid id);

    /// <summary>
    /// Whitelists the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to be whitelisted.</param>
    /// <param name="reason">The reason for whitelisting the IP address.</param>
    /// <throws><see cref="IpRepositoryException"/> if the IP address is already whitelisted.</throws>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task WhitelistIpAsync(string ip, string reason);

    /// <summary>
    /// Change status of shouldHideIfBanned for the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to be changed.</param>
    /// <param name="shouldHide">The new value of shouldHideIfBanned.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task ChangeShouldHideIfBannedAsync(string ip, bool shouldHide);

    /// <summary>
    /// Add tokens associated with ip
    /// </summary>
    /// <param name="ip">IP string</param>
    /// <param name="tokenId">Id of token address</param>
    /// <throws><see cref="IpRepositoryException"/> If the IP address or some of the tokens are not found.</throws>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddTokenAsyncIfNeeded(string ip, Guid tokenId);

    /// <summary>
    /// Connects user request with ip
    /// </summary>
    /// <param name="ip">IP string</param>
    /// <param name="userRequestId">Id of user request</param>
    /// <throws><see cref="IpRepositoryException"/> if the IP address or user request is not found.</throws>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddUserRequestToIpAsync(string ip, Guid userRequestId);
    
    /// <summary>
    /// Find tokens by ip
    /// </summary>
    /// <param name="ip">IP string</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the list of found <see cref="TokenInfo"/> objects.</returns>
    public Task<List<TokenInfo>> FindTokensByIpAsync(string ip);
}