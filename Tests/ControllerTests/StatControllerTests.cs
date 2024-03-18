using Kyoto.Controllers;
using Kyoto.Models;
using Kyoto.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Tests;

public class StatControllerTests
{
    private readonly StatController _controller;
    private readonly UserRequestRepositoryFake _userRequestRepository;
    private const string defaultHost = "example.com";
    private const string defaultPath = "/test/path/";
    private const string defaultIp = "192.168.0.1";

    public StatControllerTests()
    {
        _userRequestRepository = new UserRequestRepositoryFake();
        _controller = new StatController(_userRequestRepository);
    }

    [Fact]
    public async Task GetTodayStats_WithNoRequests_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetTodayStats();
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        Assert.Empty((IEnumerable<UserRequest>) okResult.Value!);
    }
    
    [Fact]
    public async Task GetTodayStats_WithRequests_ReturnsRequests()
    {
        // Arrange
        var request = MockUserRequest(DateTime.UtcNow);
        await _userRequestRepository.SaveUserRequestAsync(request);
        
        // Act
        var result = await _controller.GetTodayStats();
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        var requests = (List<UserRequest>) okResult.Value!;
        Assert.Single(requests);
        Assert.Equal(request, requests.First());
    }
    
    [Fact]
    public async Task GetTodayStats_GivenTodaysRequestAndEarlierRequest_ReturnsTodaysRequest()
    {
        // Arrange
        var todayRequest = MockUserRequest(DateTime.UtcNow);
        var earlierRequest = MockUserRequest(DateTime.UtcNow.AddDays(-2));
        await _userRequestRepository.SaveUserRequestAsync(todayRequest);
        await _userRequestRepository.SaveUserRequestAsync(earlierRequest);
        
        // Act
        var result = await _controller.GetTodayStats();
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        var requests = (List<UserRequest>) okResult.Value!;
        Assert.Single(requests);
        Assert.Equal(todayRequest, requests.First());
    }
    
    [Fact]
    public async Task GetTodayStats_GivenNotBlockedRequest_ShouldReturnEmptyList()
    {
        // Arrange
        var request = MockUserRequest(DateTime.UtcNow);
        request.IsBlocked = false;
        await _userRequestRepository.SaveUserRequestAsync(request);
        
        // Act
        var result = await _controller.GetTodayStats();
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        Assert.Empty((IEnumerable<UserRequest>) okResult.Value!);
    }
    
    [Fact]
    public async Task GetTodayStats_GivenDifferentHostAndPath_ShouldReturnEmptyList()
    {
        // Arrange
        var request = MockUserRequest(DateTime.UtcNow, "another.com", "/another/path/");
        await _userRequestRepository.SaveUserRequestAsync(request);
        
        // Act
        var result = await _controller.GetTodayStats("example.com", "/test/path/");
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        Assert.Empty((IEnumerable<UserRequest>) okResult.Value!);
    }
    
    [Fact]
    public async Task GetTodayStats_GivenDifferentHostAndSamePath_ShouldReturnEmptyList()
    {
        // Arrange
        var request = MockUserRequest(DateTime.UtcNow, "another.com");
        await _userRequestRepository.SaveUserRequestAsync(request);
        
        // Act
        var result = await _controller.GetTodayStats(defaultHost, defaultPath);
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        Assert.Empty((IEnumerable<UserRequest>) okResult.Value!);
    }
    
    [Fact]
    public async Task GetTodayStats_GivenSameHostAndDifferentPath_ShouldReturnEmptyList()
    {
        // Arrange
        var request = MockUserRequest(DateTime.UtcNow, path: "/another/path/");
        await _userRequestRepository.SaveUserRequestAsync(request);
        
        // Act
        var result = await _controller.GetTodayStats(defaultHost, defaultPath);
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        Assert.Empty((IEnumerable<UserRequest>) okResult.Value!);
    }
    
    [Fact]
    public async Task GetTodayStats_GivenWildcardHostAndSpecificPath_ShouldReturnValidRequests()
    {
        // Arrange
        var request1 = MockUserRequest(DateTime.UtcNow.AddMinutes(-1), "host-1.com", defaultPath);
        var request2 = MockUserRequest(DateTime.UtcNow.AddMinutes(-2), "host-2.com", defaultPath);
        var request3 = MockUserRequest(DateTime.UtcNow.AddMinutes(-3), "host-3.com", "/some/path/");
        await _userRequestRepository.SaveUserRequestAsync(request1);
        await _userRequestRepository.SaveUserRequestAsync(request2);
        await _userRequestRepository.SaveUserRequestAsync(request3);
        
        // Act
        var result = await _controller.GetTodayStats(path: defaultPath);
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        var requests = (List<UserRequest>) okResult.Value!;
        Assert.Equal(2, requests.Count);
        Assert.Equal(request1, requests.First());
        Assert.Equal(request2, requests.Last());
    }
    
    [Fact]
    public async Task GetTodayStats_GivenSpecificPathAndWildcardHost_ShouldReturnValidRequests()
    {
        // Arrange
        var request1 = MockUserRequest(DateTime.UtcNow.AddMinutes(-1), defaultHost, "/path-1/");
        var request2 = MockUserRequest(DateTime.UtcNow.AddMinutes(-2), defaultHost, "/path-2/");
        var request3 = MockUserRequest(DateTime.UtcNow.AddMinutes(-3), "another.com", "/path-3/");
        await _userRequestRepository.SaveUserRequestAsync(request1);
        await _userRequestRepository.SaveUserRequestAsync(request2);
        await _userRequestRepository.SaveUserRequestAsync(request3);
        
        // Act
        var result = await _controller.GetTodayStats("*", "/path-2/");
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult) result;
        var requests = (List<UserRequest>) okResult.Value!;
        Assert.Single(requests);
        Assert.Equal(request2, requests.First());
    }

    
    private UserRequest MockUserRequest(
        DateTime requestDate,
        string host = defaultHost,
        string path = defaultPath,
        string ip = defaultIp)
    {
        return new UserRequest(
            Guid.NewGuid(),
            null,
            host,
            path,
            new Dictionary<string, string>
            {
                { "Referer", "http://www.example.com" },
                { "User-Agent", "Mozilla/5.0" }
            })
        {
            RequestDate = requestDate,
            IsBlocked = true
        };
    }
}