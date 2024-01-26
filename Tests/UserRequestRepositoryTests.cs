using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Implementations;

namespace Tests;

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
        var userRequest = MockUserRequest();
        
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
        var userRequest = MockUserRequest();
        await  _userRequestRepository.SaveUserRequestAsync(userRequest);
        var userRequests = await _dbContext.UserRequests.ToListAsync();
        
        // Act
        await _userRequestRepository.SaveUserRequestAsync(userRequest);
        
        // Assert
        Assert.Single(userRequests);
        Assert.Equal(userRequest, userRequests[0]);
    }

    private static UserRequest MockUserRequest(
        Guid id = default,
        DateTime requestDate = default,
        string token = "testToken",
        string ip = "192.168.0.1",
        string host = "test-host.com",
        string path = "/test/path",
        string userAgent = "testUserAgent",
        Dictionary<string, string>? headers = null)
    {
        return new UserRequest(
            id,
            requestDate,
            ip,
            token,
            userAgent,
            host,
            path,
            headers ?? new Dictionary<string, string>());
    }

    private static List<UserRequest> MockUserRequests()
    {
        return new List<UserRequest>
        {
            MockUserRequest(id: Guid.Parse("00000000-0000-0000-0000-000000000001")),
            MockUserRequest(id: Guid.Parse("00000000-0000-0000-0000-000000000002")),
            MockUserRequest(id: Guid.Parse("00000000-0000-0000-0000-000000000003"))
        };
    }
}