using Microsoft.EntityFrameworkCore;
using Kyoto.Exceptions;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Implementations;
using Tests.Helpers;

namespace Tests.RepositoryTests;

public class RestrictedUrlRepositoryTests
{
    private readonly RestrictedUrlRepository _restrictedUrlRepository;
    private readonly KyotoDbContext _dbContext;

    public RestrictedUrlRepositoryTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        _restrictedUrlRepository = new RestrictedUrlRepository(_dbContext);
    }

    ~RestrictedUrlRepositoryTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }

    [Fact]
    public async Task BlockUrl_ShouldBlockUrl()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";

        // Act
        await _restrictedUrlRepository.BlockUrl(host, path, reason);

        // Assert
        var url = await _dbContext.RestrictedUrls.FirstOrDefaultAsync(ru => ru.Host == host && ru.Path == path);
        Assert.NotNull(url);
        Assert.Equal(reason, url.Reason);
    }

    [Fact]
    public async Task BlockUrl_ShouldNotBlockAlreadyBlockedUrl()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl(host, path, reason);

        // Act & Assert
        await Assert.ThrowsAsync<RestrictedUrlRepositoryException>(() =>
            _restrictedUrlRepository.BlockUrl(host, path, reason));
    }

    [Fact]
    public async Task UnblockUrl_ShouldUnblockUrl()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl(host, path, reason);

        // Act
        await _restrictedUrlRepository.UnblockUrl(host, path);

        // Assert
        var url = await _dbContext.RestrictedUrls.FirstOrDefaultAsync(ru => ru.Host == host && ru.Path == path);
        Assert.Null(url);
    }

    [Fact]
    public async Task UnblockUrl_ShouldNotUnblockNotBlockedUrl()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";

        // Act & Assert
        await Assert.ThrowsAsync<RestrictedUrlRepositoryException>(
            () => _restrictedUrlRepository.UnblockUrl(host, path));
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnTrueIfUrlIsBlocked()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl(host, path, reason);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnFalseIfUrlIsNotBlocked()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnFalseIfUrlIsUnblocked()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl(host, path, reason);
        await _restrictedUrlRepository.UnblockUrl(host, path);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnTrueIfUrlIsBlockedUntilDate()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        var bannedUntil = DateTime.UtcNow.AddDays(1);
        await _restrictedUrlRepository.BlockUrl(host, path, reason, bannedUntil);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnFalseIfUrlIsBlockedUntilDateIsPassed()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        var bannedUntil = DateTime.UtcNow.AddDays(-1);
        await _restrictedUrlRepository.BlockUrl(host, path, reason, bannedUntil);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnFalseIfUrlIsBlockedUntilDateIsToday()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        var bannedUntil = DateTime.UtcNow;
        await _restrictedUrlRepository.BlockUrl(host, path, reason, bannedUntil);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnFalseIfUrlIsBlockedWithUntilDateInFutureAndUnblocked()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        var bannedUntil = DateTime.UtcNow.AddDays(1);
        await _restrictedUrlRepository.BlockUrl(host, path, reason, bannedUntil);
        await _restrictedUrlRepository.UnblockUrl(host, path);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_ShouldReturnFalseIfUrlIsBlockedWithUntilDateInPastAndUnblocked()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        var bannedUntil = DateTime.UtcNow.AddDays(-1);
        await _restrictedUrlRepository.BlockUrl(host, path, reason, bannedUntil);
        await _restrictedUrlRepository.UnblockUrl(host, path);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_TestWildcardHost_ShouldReturnTrueIfUrlIsBlocked()
    {
        // Arrange
        const string host = "www.example.com";
        var path = Guid.NewGuid().ToString();
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl("*", path, reason);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_TestWildcardPath_ShouldReturnTrueIfUrlIsBlocked()
    {
        // Arrange
        const string host = "www.example.com";
        var path = Guid.NewGuid().ToString();
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl(host, "*", reason);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_TestWildcardHostAndPath_ShouldReturnTrueIfUrlIsBlocked()
    {
        // Arrange
        const string host = "www.example.com";
        var path = Guid.NewGuid().ToString();
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl("*", "*", reason);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, path);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public async Task IsUrlBlocked_TestWildcardHost_ShouldFalseIfPathIsDifferent()
    {
        // Arrange
        const string host = "www.example.com";
        const string path = "/restricted";
        const string reason = "Test";
        await _restrictedUrlRepository.BlockUrl("*", path, reason);

        // Act
        var isBlocked = await _restrictedUrlRepository.IsUrlBlocked(host, "/not-restricted");

        // Assert
        Assert.False(isBlocked);
    }
}