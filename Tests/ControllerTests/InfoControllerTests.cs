using Kyoto.Controllers;
using Kyoto.Models;
using Kyoto.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Tests;

public class InfoControllerTests
{
    private IpRepositoryFake _ipRepository;
    private TokenRepositoryFake _tokenRepository;
    private UserRequestRepositoryFake _userRequestRepository;
    private InfoController _infoController;
    private const string DefaultIp = "192.168.0.1";
    private const string DefaultToken = "TestToken";
    
    public InfoControllerTests()
    {
        _userRequestRepository = new UserRequestRepositoryFake();
        _tokenRepository = new TokenRepositoryFake(_userRequestRepository);
        _ipRepository = new IpRepositoryFake(_userRequestRepository, _tokenRepository);
        _infoController = new InfoController(_ipRepository, _tokenRepository, _userRequestRepository);
    }
    
    [Fact]
    public async Task GetIpInfo_ShouldReturnIpInfo()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        _ipRepository.Ips.Add(ipInfo);
        
        // Act
        var result = await _infoController.GetIpInfo(DefaultIp) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(ipInfo, response?.ipInfo);
        Assert.Empty(response?.relatedTokens);
    }
    
    [Fact]
    public async Task GetTokenInfo_ShouldReturnTokenInfo()
    {
        // Arrange
        var tokenInfo = new TokenInfo(DefaultToken);
        _tokenRepository.Tokens.Add(tokenInfo);
        
        // Act
        var result = await _infoController.GetTokenInfo(DefaultToken) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(tokenInfo, response?.tokenInfo);
        Assert.Empty(response?.relatedIps);
    }
    
    [Fact]
    public async Task GetIpInfo_GivenInvalidIp_ShouldReturnNotFound()
    {
        // Act
        var result = await _infoController.GetIpInfo("InvalidIp") as NotFoundResult;
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    
    [Fact]
    public async Task GetTokenInfo_GivenInvalidToken_ShouldReturnNotFound()
    {
        // Act
        var result = await _infoController.GetTokenInfo("InvalidToken") as NotFoundResult;
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    
    [Fact]
    public async Task GetIpInfo_ShouldReturnRelatedTokens()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        var tokenInfo = new TokenInfo(DefaultToken);
        _ipRepository.Ips.Add(ipInfo);
        _ipRepository.IpTokens.Add(new IpToken(ipInfo.Id, tokenInfo.Id));
        _tokenRepository.Ips.Add(ipInfo);
        _tokenRepository.Tokens.Add(tokenInfo);
        await _tokenRepository.AddIpAddressIfNeededAsync(DefaultToken, ipInfo.Id);
        
        // Act
        var result = await _infoController.GetIpInfo(DefaultIp) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(ipInfo, response?.ipInfo);
        Assert.Single(response?.relatedTokens);
        Assert.Equal(tokenInfo, response?.relatedTokens[0]);
    }
    
    [Fact]
    public async Task GetTokenInfo_ShouldReturnRelatedIps()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        var tokenInfo = new TokenInfo(DefaultToken);
        _ipRepository.Ips.Add(ipInfo);
        _tokenRepository.IpTokens.Add(new IpToken(ipInfo.Id, tokenInfo.Id));
        _tokenRepository.Ips.Add(ipInfo);
        _tokenRepository.Tokens.Add(tokenInfo);
        await _tokenRepository.AddIpAddressIfNeededAsync(DefaultToken, ipInfo.Id);
        
        // Act
        var result = await _infoController.GetTokenInfo(DefaultToken) as OkObjectResult;
        var response = result?.Value as dynamic;
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(tokenInfo, response?.tokenInfo);
        Assert.Single(response?.relatedIps);
        Assert.Equal(ipInfo, response?.relatedIps[0]);
    }
}