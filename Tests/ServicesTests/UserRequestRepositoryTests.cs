using Microsoft.EntityFrameworkCore;
using Smug.Exceptions;
using Tests.Helpers;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Implementations;

namespace Tests.RepositoryTests;

public class UserRequestRepositoryTests
{
    private readonly SmugDbContext _dbContext;
    private readonly UserRequestRepository _userRequestRepository;

    public UserRequestRepositoryTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        _userRequestRepository = new UserRequestRepository(_dbContext);
    }

    ~UserRequestRepositoryTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }

    [Fact]
    public async Task SaveUserRequestAsync_ShouldSaveNewUserRequest()
    {
        // Arrange
        var userRequest = await MockUserRequest();

        // Act
        await _userRequestRepository.SaveUserRequestAsync(userRequest);
        var userRequests = await _dbContext.UserRequests.ToListAsync();

        // Assert
        Assert.Single(userRequests);
        Assert.Equal(userRequest, userRequests[0]);
    }

    [Fact]
    public async Task SaveUserRequestAsync_ShouldNotSaveExistingUserRequest()
    {
        // Arrange
        var userRequest = await MockUserRequest();
        await _userRequestRepository.SaveUserRequestAsync(userRequest);
        var userRequests = await _dbContext.UserRequests.ToListAsync();

        // Act
        await _userRequestRepository.SaveUserRequestAsync(userRequest);

        // Assert
        Assert.Single(userRequests);
        Assert.Equal(userRequest, userRequests[0]);
    }

    [Fact]
    public async Task FindUserRequestAsync_ShouldFindUserRequestById()
    {
        // Arrange
        var userRequests = await MockUserRequests();
        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var userRequest = await _userRequestRepository.FindUserRequestAsync(userRequests[1].Id);

        // Assert
        Assert.Equal(userRequests[1], userRequest);
    }

    [Fact]
    public async Task FindUserRequestAsync_ShouldReturnNullIfUserRequestNotFoundById()
    {
        // Arrange
        var userRequests = await MockUserRequests();
        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var userRequest = await _userRequestRepository.FindUserRequestAsync(Guid.NewGuid());

        // Assert
        Assert.Null(userRequest);
    }

    [Fact]
    public async Task FindUserRequestsAsync_ShouldFindUserRequestsByToken()
    {
        // Arrange
        var userRequests = new List<UserRequest>();

        for (var i = 0; i < 3; i++) userRequests.Add(await MockUserRequest(token: $"testToken-{i}"));

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .FindUserRequestByTokenAsync(userRequests[1].TokenInfo!.Token);

        // Assert
        Assert.Single(foundUserRequests);
        Assert.Equal(userRequests[1], foundUserRequests[0]);
    }

    [Fact]
    public async Task FindUserRequestsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByToken()
    {
        // Arrange
        var userRequests = new List<UserRequest>();

        for (var i = 0; i < 3; i++) userRequests.Add(await MockUserRequest(token: $"testToken-{i}"));

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository.FindUserRequestByTokenAsync("notFoundToken");

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task FindUserRequestsAsync_ShouldFindUserRequestsByIp()
    {
        // Arrange
        var userRequests = new List<UserRequest>();

        for (var i = 0; i < 3; i++) userRequests.Add(await MockUserRequest(ip: $"192.168.0.{i}"));

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository.FindUserRequestByIpAsync(userRequests[1].IpInfo.Ip);

        // Assert
        Assert.Single(foundUserRequests);
        Assert.Equal(userRequests[1], foundUserRequests[0]);
    }

    [Fact]
    public async Task FindUserRequestsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByIp()
    {
        // Arrange
        var userRequests = new List<UserRequest>();

        for (var i = 0; i < 3; i++) userRequests.Add(await MockUserRequest(ip: $"192.168.0.{i}"));

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository.FindUserRequestByIpAsync("1.2.3.4");

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task GetUserRequestsOnEndPointsAsync_ShouldFindUserRequestsByHostPathAndDate()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 3;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(i))
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetUserRequestsOnEndPointsAsync("google.com", "some/path", DateTime.UtcNow.AddHours(-12));

        // Assert
        Assert.Equal(requestsCount, foundUserRequests.Count);
        Assert.Equal(userRequests, foundUserRequests);
    }

    [Fact]
    public async Task GetUserRequestsOnEndPointsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByDate()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 3;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i))
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetUserRequestsOnEndPointsAsync("google.com", "some/path", DateTime.UtcNow.AddHours(1));

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task GetUserRequestsOnEndPointsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByPath()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 3;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i))
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetUserRequestsOnEndPointsAsync("google.com", "some/other/path", DateTime.UtcNow.AddHours(-12));

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task GetUserRequestsOnEndPointsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByHost()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 3;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i))
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetUserRequestsOnEndPointsAsync("some-other-host.com", "some/path", DateTime.UtcNow.AddHours(-12));

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task GetBlockedRequestsAsync_ShouldFindBlockedUserRequestsByHostPathAndDate()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 10;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i),
                isBlocked: i % 2 == 0)
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetBlockedRequestsAsync("google.com", "some/path", DateTime.UtcNow.AddHours(-12));

        // Assert
        Assert.Equal(requestsCount / 2, foundUserRequests.Count);
        Assert.Equal(userRequests.Where(ur => ur.IsBlocked), foundUserRequests);
    }

    [Fact]
    public async Task GetBlockedRequestsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByDate()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 10;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i),
                isBlocked: i % 2 == 0)
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetBlockedRequestsAsync("google.com", "some/path", DateTime.UtcNow.AddHours(1));

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task GetBlockedRequestsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByPath()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 10;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i),
                isBlocked: i % 2 == 0)
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetBlockedRequestsAsync("google.com", "some/other/path", DateTime.UtcNow.AddHours(-12));

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task GetBlockedRequestsAsync_ShouldReturnEmptyListIfUserRequestsNotFoundByHost()
    {
        // Arrange
        var userRequests = new List<UserRequest>();
        const int requestsCount = 10;

        for (var i = 0; i < requestsCount; i++)
            userRequests.Add(await MockUserRequest(
                ip: $"192.168.0.{i}",
                token: $"testToken-{i}",
                requestDate: DateTime.UtcNow.AddHours(-i),
                isBlocked: i % 2 == 0)
            );

        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();

        // Act
        var foundUserRequests = await _userRequestRepository
            .GetBlockedRequestsAsync("some-other-host.com", "some/path", DateTime.UtcNow.AddHours(-12));

        // Assert
        Assert.Empty(foundUserRequests);
    }

    [Fact]
    public async Task UpdateUserRequestAsync_ShouldUpdateUserRequest()
    {
        // Arrange
        var userRequest = await MockUserRequest();
        await _dbContext.UserRequests.AddAsync(userRequest);
        await _dbContext.SaveChangesAsync();

        userRequest.IsBlocked = true;
        userRequest.DecisionReason = "testReason";

        // Act
        await _userRequestRepository.UpdateUserRequestAsync(userRequest);
        var updatedUserRequest = await _dbContext.UserRequests.FindAsync(userRequest.Id);

        // Assert
        Assert.True(updatedUserRequest!.IsBlocked);
        Assert.Equal("testReason", updatedUserRequest.DecisionReason);
    }

    [Fact]
    public async Task UpdateUserRequestAsync_ShouldThrowIfUserRequestNotFound()
    {
        // Arrange
        var userRequest = await MockUserRequest();

        // Act & Assert
        await Assert.ThrowsAsync<UserRequestRepositoryException>(
            () => _userRequestRepository.UpdateUserRequestAsync(userRequest));
    }

    private async Task<UserRequest> MockUserRequest(
        Guid id = default,
        string? token = "testToken",
        string ip = "192.168.0.1",
        string host = "google.com",
        string path = "some/path",
        DateTime? requestDate = null,
        bool isBlocked = false)
    {
        var ipInfo = new IpAddressInfo(ip);
        TokenInfo? tokenInfo = null;

        if (token != null)
        {
            tokenInfo = new TokenInfo(token);
            await _dbContext.Tokens.AddAsync(tokenInfo);
        }

        await _dbContext.Ips.AddAsync(ipInfo);

        var userRequest = new UserRequest(
            id,
            requestDate ?? DateTime.UtcNow,
            ipInfo.Id,
            tokenInfo?.Id,
            host,
            path,
            new Dictionary<string, string>());

        userRequest.IsBlocked = isBlocked;

        return userRequest;
    }

    private async Task<List<UserRequest>> MockUserRequests()
    {
        return new List<UserRequest>
        {
            await MockUserRequest(Guid.Parse("00000000-0000-0000-0000-000000000001")),
            await MockUserRequest(Guid.Parse("00000000-0000-0000-0000-000000000002")),
            await MockUserRequest(Guid.Parse("00000000-0000-0000-0000-000000000003"))
        };
    }
}