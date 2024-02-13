using Microsoft.EntityFrameworkCore;
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
        
        for (var i = 0; i < 3; i++)
        {
            userRequests.Add(await MockUserRequest(token: $"testToken-{i}"));
        }
        
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
        
        for (var i = 0; i < 3; i++)
        {
            userRequests.Add(await MockUserRequest(token: $"testToken-{i}"));
        }
        
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
        
        for (var i = 0; i < 3; i++)
        {
            userRequests.Add(await MockUserRequest(ip: $"192.168.0.{i}"));
        }
        
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
        
        for (var i = 0; i < 3; i++)
        {
            userRequests.Add(await MockUserRequest(ip: $"192.168.0.{i}"));
        }
        
        await _dbContext.UserRequests.AddRangeAsync(userRequests);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var foundUserRequests = await _userRequestRepository.FindUserRequestByIpAsync("1.2.3.4");
        
        // Assert
        Assert.Empty(foundUserRequests);
    }
    
    private async Task<UserRequest> MockUserRequest(
        Guid id = default,
        string? token = "testToken",
        string ip = "192.168.0.1")
    {
        var ipInfo = new IpAddressInfo(ip);
        TokenInfo? tokenInfo = null;
        
        if (token != null) {
            tokenInfo = new TokenInfo(token);
            await _dbContext.Tokens.AddAsync(tokenInfo);
        }
        await _dbContext.Ips.AddAsync(ipInfo);
        
        return new UserRequest(
            id,
            DateTime.UtcNow, 
            ipInfo.Id,
            tokenInfo?.Id,
            "google.com",
            "some/path",
            new Dictionary<string, string>());
    }
    
    private async Task<List<UserRequest>> MockUserRequests()
    {
        return new List<UserRequest>
        {
            await MockUserRequest(id: Guid.Parse("00000000-0000-0000-0000-000000000001")),
            await MockUserRequest(id: Guid.Parse("00000000-0000-0000-0000-000000000002")),
            await MockUserRequest(id: Guid.Parse("00000000-0000-0000-0000-000000000003"))
        };
    }
}