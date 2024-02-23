using Smug.Exceptions;
using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Tests.Fakes;

public class RestrictedUrlRepositoryFake : IRestrictedUrlRepository
{
    public List<RestrictedUrl> RestrictedUrls { get; private set; } = new();
    public int BlockUrlCount { get; private set; } = 0;
    public int UnblockUrlCount { get; private set; } = 0;
    public int IsUrlBlockedCount { get; private set; } = 0;

    public Task BlockUrl(string host, string path, string reason, DateTime? bannedUntil = null)
    {
        BlockUrlCount++;
        if (RestrictedUrls.Any(ru => ru.Host == host && ru.Path == path))
            throw new RestrictedUrlRepositoryException($"URL {host}{path} is already blocked");

        var restrictedUrl = new RestrictedUrl(host, path, reason, bannedUntil);

        RestrictedUrls.Add(restrictedUrl);
        return Task.CompletedTask;
    }

    public Task UnblockUrl(string host, string path)
    {
        UnblockUrlCount++;
        var restrictedUrl = RestrictedUrls.FirstOrDefault(ru => ru.Host == host && ru.Path == path);

        if (restrictedUrl == null) throw new RestrictedUrlRepositoryException($"URL {host}{path} is not blocked");

        RestrictedUrls.Remove(restrictedUrl);
        return Task.CompletedTask;
    }

    public Task<bool> IsUrlBlocked(string host, string path)
    {
        IsUrlBlockedCount++;
        var restrictedUrl = RestrictedUrls
            .Where(ru => ru.Host == "*" || ru.Host == host)
            .Where(ru => ru.Path == "*" || ru.Path == path)
            .FirstOrDefault();

        if (restrictedUrl == null) return Task.FromResult(false);

        if (!restrictedUrl.BannedUntil.HasValue) return Task.FromResult(true);

        var isBanned = restrictedUrl.BannedUntil.Value > DateTime.UtcNow;

        if (!isBanned) RestrictedUrls.Remove(restrictedUrl);

        return Task.FromResult(isBanned);
    }
}