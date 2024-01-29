using Smug.Models;
using Smug.Exceptions;

namespace Smug.Services.Interfaces;

public interface IUrlRepository
{
    /// <summary>
    /// Restricts access to the URL for everybody except whitelisted IPs and tokens
    /// </summary>
    /// <param name="host">Host of the URL</param>
    /// <param name="path">Path of the URL</param>
    /// <param name="reason">Reason for restricting the URL</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the <see cref="RestrictedUrl"/> object.</returns>
    public Task<RestrictedUrl> RestrictUrlAsync(string host, string path, string reason);
    
    /// <summary>
    /// Removes restriction from the URL
    /// </summary>
    /// <param name="host">Host of the URL</param>
    /// <param name="path">Path of the URL</param>
    /// <exception cref="UrlRepositoryException">Thrown if URL is not restricted</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RemoveRestrictionAsync(string host, string path);
    
    /// <summary>
    /// Finds restricted URL by host and path
    /// </summary>
    /// <param name="host">Host of the URL</param>
    /// <param name="path">Path of the URL</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is the <see cref="RestrictedUrl"/> object, or null if not found.</returns>
    public Task<RestrictedUrl?> FindRestrictedUrlAsync(string host, string path);
}