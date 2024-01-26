using Smug.Models;

namespace Smug.Services.Interfaces;

public interface ITokenRepository
{
    /// <summary>
    /// Save token if needed in database
    /// </summary>
    /// <param name="token"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the newly created <see cref="TokenInfo"/> object.</returns>
    public Task<TokenInfo> SaveTokenAsync(string token);
    /// <summary>
    /// Save token if needed in database and ban it
    /// </summary>
    /// <param name="token"></param>
    /// <param name="reason"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the banned <see cref="TokenInfo"/> object.</returns>
    public Task<TokenInfo> BanTokenAsync(string token, string? reason);
    /// <summary>
    /// Unban token
    /// </summary>
    /// <param name="token"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task UnbanTokenAsync(string token);
    /// <summary>
    /// Find token by token string
    /// </summary>
    /// <param name="id"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="TokenInfo"/> object, or null if not found.</returns>
    public Task<TokenInfo?> FindTokenAsync(Guid id);
    /// <summary>
    /// Find token by id
    /// </summary>
    /// <param name="token"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the found <see cref="TokenInfo"/> object, or null if not found.</returns>
    public Task<TokenInfo?> FindTokenAsync(string token);
}