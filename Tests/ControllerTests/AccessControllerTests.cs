using Kyoto.Controllers;
using Kyoto.Models;
using Kyoto.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Tests;

public class AccessControllerTests
{
    private IpRepositoryFake _ipRepository;
    private TokenRepositoryFake _tokenRepository;
    private UserRequestRepositoryFake _userRequestRepository;
    private AccessController _accessController;
    private const string DefaultIp = "192.168.0.1";
    private const string DefaultToken = "TestToken";
    private const string DefaultReason = "Test";

    public AccessControllerTests()
    {
        _userRequestRepository = new UserRequestRepositoryFake();
        _tokenRepository = new TokenRepositoryFake(_userRequestRepository);
        _ipRepository = new IpRepositoryFake(_userRequestRepository, _tokenRepository);
        _accessController = new AccessController(_ipRepository, _tokenRepository, _userRequestRepository);
    }

    [Fact]
    public async Task BanIp_ShouldBanIp()
    {
        // Act
        var result = await _accessController.BanIp(DefaultIp, DefaultReason) as OkObjectResult;
        var bannedIp = result?.Value as IpAddressInfo;

        // Assert
        Assert.NotNull(bannedIp);
        Assert.Equal(IpStatus.Banned, bannedIp.Status);
        Assert.Equal(DefaultIp, bannedIp.Ip);
        Assert.Equal(DefaultReason, bannedIp.StatusChangeReason);
        Assert.Equal(1, _ipRepository.BanIpIfNeededAsync2ParamsCount);
        Assert.Contains(bannedIp, _ipRepository.Ips);
        Assert.Single(_ipRepository.Ips);
    }

    [Fact]
    public async Task BanToken_ShouldBanToken()
    {
        // Act
        var result = await _accessController.BanToken(DefaultToken, DefaultReason) as OkObjectResult;
        var bannedToken = result?.Value as TokenInfo;

        // Assert
        Assert.NotNull(bannedToken);
        Assert.Equal(TokenStatus.Banned, bannedToken.Status);
        Assert.Equal(DefaultToken, bannedToken.Token);
        Assert.Equal(DefaultReason, bannedToken.StatusChangeReason);
        Assert.Equal(1, _tokenRepository.BanTokenAsyncCount);
        Assert.Contains(bannedToken, _tokenRepository.Tokens);
        Assert.Single(_tokenRepository.Tokens);
    }
    
    [Fact]
    public async Task BanIp_GivenInvalidIp_ShouldReturnBadRequest()
    {
        // Act
        var result = await _accessController.BanIp("InvalidIp", DefaultReason) as BadRequestObjectResult;
        var response = result?.Value as dynamic;

        // Assert
        Assert.NotNull(response);
        Assert.True(response?.error);
        Assert.Equal("Invalid IP address", response?.description);
        Assert.Equal("InvalidIp", response?.ip);
        Assert.Equal(0, _ipRepository.BanIpIfNeededAsync2ParamsCount);
        Assert.Empty(_ipRepository.Ips);
    }

    [Fact]
    public async Task UnbanIp_ShouldUnbanIp()
    {
        // Arrange
        await _ipRepository.BanIpIfNeededAsync(DefaultIp, DefaultReason);

        // Act
        var result = await _accessController.UnbanIp(DefaultIp, DefaultReason) as OkObjectResult;
        var response = result?.Value as dynamic;

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, _ipRepository.UnbanIpAsyncCount);
        Assert.Single(_ipRepository.Ips);
        Assert.Equal(IpStatus.Normal, _ipRepository.Ips[0].Status);
        Assert.Equal(DefaultIp, response?.ip);
    }

    [Fact]
    public async Task BanIp_InvalidIp_ShouldReturnBadRequest()
    {
        // Act
        var response = await _accessController.BanIp("InvalidIp", DefaultReason) as BadRequestObjectResult;
        var result = response?.Value as dynamic;

        // Assert
        Assert.NotNull(result);
        Assert.True(result?.error);
        Assert.Equal("Invalid IP address", result?.description);
        Assert.Equal("InvalidIp", result?.ip);
        Assert.Equal(0, _ipRepository.BanIpIfNeededAsync2ParamsCount);
        Assert.Empty(_ipRepository.Ips);
    }

    [Fact]
    public async Task UnbanIp_InvalidIp_ShouldReturnBadRequest()
    {
        // Act
        var result = await _accessController.UnbanIp("InvalidIp", DefaultReason) as BadRequestObjectResult;
        var response = result?.Value as dynamic;

        // Assert
        Assert.NotNull(response);
        Assert.True(response?.error);
        Assert.Equal("Invalid IP address", response?.description);
        Assert.Equal("InvalidIp", response?.ip);
        Assert.Equal(0, _ipRepository.UnbanIpAsyncCount);
    }

    [Fact]
    public async Task BanToken_ShouldBanTokenAndIp()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        var tokenInfo = new TokenInfo(DefaultToken);
        _ipRepository.Ips.Add(ipInfo);
        _tokenRepository.Tokens.Add(tokenInfo);
        const string host = "example.com";
        const string path = "/some/path/";
        var userRequest = new UserRequest(ipInfo.Id, tokenInfo.Id, host, path);
        
        _userRequestRepository.UserRequests.Add(userRequest);
        
        // Act
        var result = await _accessController.BanToken(DefaultToken, DefaultReason) as OkObjectResult;
        var bannedToken = result?.Value as TokenInfo;
        
        // Assert
        Assert.NotNull(bannedToken);
        Assert.Equal(TokenStatus.Banned, bannedToken.Status);
        Assert.Equal(DefaultToken, bannedToken.Token);
        Assert.Equal(DefaultReason, bannedToken.StatusChangeReason);
        Assert.Equal(1, _tokenRepository.BanTokenAsyncCount);
        Assert.Contains(bannedToken, _tokenRepository.Tokens);
        Assert.Single(_tokenRepository.Tokens);
        Assert.Contains(ipInfo, _ipRepository.Ips);
        Assert.Single(_ipRepository.Ips);
        Assert.Equal(1, _userRequestRepository.FindUserRequestByTokenAsyncCount);
        Assert.Contains(userRequest, _userRequestRepository.UserRequests);
        Assert.Single(_userRequestRepository.UserRequests);
    }
    
    [Fact]
    public async Task UnbanToken_ShouldUnbanTokenAndIp()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        var tokenInfo = new TokenInfo(DefaultToken);
        tokenInfo.UpdateStatus(TokenStatus.Banned, DefaultReason);
        ipInfo.UpdateStatus(IpStatus.Banned, DefaultReason);
        _ipRepository.Ips.Add(ipInfo);
        _tokenRepository.Tokens.Add(tokenInfo);
        var userRequest = new UserRequest(ipInfo.Id, tokenInfo.Id, "example.com", "/some/path/");
        
        _userRequestRepository.UserRequests.Add(userRequest);
        
        // Act
        var result = await _accessController.UnbanToken(DefaultToken, DefaultReason) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, _tokenRepository.UnbanTokenAsyncCount);
        Assert.Contains(tokenInfo, _tokenRepository.Tokens);
        Assert.Single(_tokenRepository.Tokens);
        Assert.Contains(ipInfo, _ipRepository.Ips);
        Assert.Single(_ipRepository.Ips);
        Assert.Equal(1, _userRequestRepository.FindUserRequestByTokenAsyncCount);
        Assert.Contains(userRequest, _userRequestRepository.UserRequests);
        Assert.Single(_userRequestRepository.UserRequests);
    }
    
    [Fact]
    public async Task UnbanIp_ShouldUnbanIpAndTokens()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        var tokenInfo = new TokenInfo(DefaultToken);
        tokenInfo.UpdateStatus(TokenStatus.Banned, DefaultReason);
        ipInfo.UpdateStatus(IpStatus.Banned, DefaultReason);
        _ipRepository.Ips.Add(ipInfo);
        _tokenRepository.Tokens.Add(tokenInfo);
        var userRequest = new UserRequest(ipInfo.Id, tokenInfo.Id, "example.com", "/some/path/");
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        _userRequestRepository.UserRequests.Add(userRequest);
        
        // Act
        var result = await _accessController.UnbanIp(ipInfo.Ip, DefaultReason) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, _ipRepository.UnbanIpAsyncCount);
        Assert.Contains(ipInfo, _ipRepository.Ips);
        Assert.Single(_ipRepository.Ips);
        Assert.Equal(1, _userRequestRepository.FindUserRequestByIpAsyncCount);
        Assert.Contains(userRequest, _userRequestRepository.UserRequests);
        Assert.Single(_userRequestRepository.UserRequests);
        Assert.Equal(DefaultIp, response?.ip);
        Assert.Single(_tokenRepository.Tokens);
        Assert.Equal(1, _tokenRepository.UnbanTokenAsyncCount);
        Assert.Equal(TokenStatus.Normal, _tokenRepository.Tokens[0].Status);
    }
    
    [Fact]
    public async Task WhitelistIp_ShouldWhitelistIp()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        _ipRepository.Ips.Add(ipInfo);
        
        // Act
        var result = await _accessController.WhitelistIp(DefaultIp, DefaultReason) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal($"IP {DefaultIp} added to white list.", response?.message);
        Assert.Equal(DefaultIp, response?.ip);
        Assert.Equal(IpStatus.Whitelisted, _ipRepository.Ips[0].Status);
    }
    
    [Fact]
    public async Task WhitelistIp_GivenInvalidIp_ShouldReturnBadRequest()
    {
        // Act
        var result = await _accessController.WhitelistIp("InvalidIp", DefaultReason) as BadRequestObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response?.error);
        Assert.Equal("Invalid IP address", response?.description);
        Assert.Equal("InvalidIp", response?.ip);
        Assert.Equal(0, _ipRepository.WhitelistIpAsyncCount);
        Assert.Empty(_ipRepository.Ips);
    }
    
    [Fact]
    public async Task WhitelistIp_GivenAlreadyWhitelistedIp_ShouldReturnBadRequest()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, DefaultReason);
        _ipRepository.Ips.Add(ipInfo);
        
        // Act
        var result = await _accessController.WhitelistIp(DefaultIp, DefaultReason) as BadRequestObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response?.error);
        Assert.Equal("IP is already whitelisted", response?.description);
        Assert.Equal(DefaultIp, response?.ip);
        Assert.Equal(1, _ipRepository.WhitelistIpAsyncCount);
        Assert.Single(_ipRepository.Ips);
    }
}