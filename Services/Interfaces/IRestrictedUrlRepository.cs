using Smug.Exceptions;

namespace Smug.Services.Interfaces;

public interface IRestrictedUrlRepository
{
    /// <summary>
    /// Blocks the URL for the specified host and path.
    /// </summary>
    /// <param name="host">The host of the URL to block.</param>
    /// <param name="path">The path of the URL to block.</param>
    /// <param name="reason">The reason for banning the URL.</param>
    /// <param name="bannedUntil">The date until the URL is banned. If null, the URL is banned indefinitely.</param>
    /// <throws><see cref="RestrictedUrlRepositoryException"/> If the URL is already blocked.</throws>
    Task BlockUrl(string host, string path, string reason, DateTime? bannedUntil);

    /// <summary>
    /// Unblocks the URL for the specified host and path.
    /// </summary>
    /// <param name="host">The host of the URL to unblock.</param>
    /// <param name="path">The path of the URL to unblock.</param>
    /// <throws><see cref="RestrictedUrlRepositoryException"/> If the URL is not blocked.</throws>
    Task UnblockUrl(string host, string path);

    /// <summary>
    /// Checks if the URL for the specified host and path is blocked.
    /// </summary>
    /// <param name="host">The host of the URL to check.</param>
    /// <param name="path">The path of the URL to check.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is true if the URL is blocked, otherwise false.</returns>
    Task<bool> IsUrlBlocked(string host, string path);
}