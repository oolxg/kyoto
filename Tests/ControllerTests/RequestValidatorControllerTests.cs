using Kyoto.Controllers;
using Kyoto.Models;
using Kyoto.Resources;
using Kyoto.Services.Implementations;
using Kyoto.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Tests;

public class RequestValidatorControllerTests
{
    private IpRepositoryFake _ipRepository;
    private TokenRepositoryFake _tokenRepository;
    private UserRequestRepositoryFake _userRequestRepository;
    private RequestValidatorController _requestValidatorController;
    private RestrictedUrlRepositoryFake _restrictedUrlRepository;
    private AccessValidatorFake _accessValidator;
    private const string DefaultIp = "192.168.0.1";
    private const string DefaultToken = "TestToken";
    private const string DefaultHost = "www.test.com";
    private const string DefaultPath = "/test/path/";
    
    public RequestValidatorControllerTests()
    {
        _userRequestRepository = new UserRequestRepositoryFake();
        _restrictedUrlRepository = new RestrictedUrlRepositoryFake();
        _tokenRepository = new TokenRepositoryFake(_userRequestRepository);
        _ipRepository = new IpRepositoryFake(_userRequestRepository, _tokenRepository);
        _accessValidator = new AccessValidatorFake();
        _requestValidatorController = new RequestValidatorController(
            _tokenRepository,
            _ipRepository,
            _userRequestRepository,
            _accessValidator);
    }

    [Fact]
    public async Task CheckRequest_GivenNoUserAgent_ShouldReturnOkWithBlockTrueAndBanAndHideIP()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        _ipRepository.Ips.Add(ipInfo);
        var request = new UserRequest(ipInfo.Id, null, DefaultHost, DefaultPath);
        _userRequestRepository.UserRequests.Add(request);
        var validationResult = new AccessValidationResult(true, AccessValidatorReasons.UserAgentIsEmpty);
        _accessValidator.AccessValidationResultToReturn = validationResult;
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserRequest"] = request;
        httpContext.Items["IpInfo"] = ipInfo;
        
        _requestValidatorController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _requestValidatorController.CheckRequest() as OkObjectResult;
        var response = result?.Value as AccessValidationResult;

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Block);
        Assert.Equal(validationResult.Reason, response.Reason);
        Assert.Equal(validationResult.Reason, _userRequestRepository.UserRequests[0].DecisionReason);
        Assert.Equal(IpStatus.Banned, _ipRepository.Ips[0].Status);
        Assert.True(_userRequestRepository.UserRequests[0].IsBlocked);
        Assert.Equal(1, _ipRepository.ChangeShouldHideIfBannedAsyncCount);
        Assert.True(_ipRepository.Ips[0].ShouldHideIfBanned);
    }
    
    [Fact]
    public async Task CheckRequest_GivenNoUserAgentAndToken_ShouldReturnOkWithBlockTrueAndBanIPAndToken()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        _ipRepository.Ips.Add(ipInfo);
        var tokenInfo = new TokenInfo(DefaultToken);
        _tokenRepository.Tokens.Add(tokenInfo);
        var request = new UserRequest(ipInfo.Id, tokenInfo.Id,DefaultHost, DefaultPath);
        _userRequestRepository.UserRequests.Add(request);
        var validationResult = new AccessValidationResult(true, AccessValidatorReasons.UserAgentIsEmpty);
        _accessValidator.AccessValidationResultToReturn = validationResult;
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserRequest"] = request;
        httpContext.Items["IpInfo"] = ipInfo;
        httpContext.Items["TokenInfo"] = tokenInfo;
        
        _requestValidatorController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _requestValidatorController.CheckRequest() as OkObjectResult;
        var response = result?.Value as AccessValidationResult;

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Block);
        Assert.Equal(validationResult.Reason, response.Reason);
        Assert.Equal(validationResult.Reason, _userRequestRepository.UserRequests[0].DecisionReason);
        Assert.Equal(IpStatus.Banned, _ipRepository.Ips[0].Status);
        Assert.True(_userRequestRepository.UserRequests[0].IsBlocked);
        Assert.Equal(TokenStatus.Banned, _tokenRepository.Tokens[0].Status);
        Assert.Equal(1, _tokenRepository.BanTokenAsyncCount);
    }
    
    [Fact]
    public async Task CheckRequest_GivenIpAndBannedToken_ShouldReturnOkWithBlockTrueAndBanIP()
    {
        // Arrange
        var ipInfo = new IpAddressInfo(DefaultIp);
        _ipRepository.Ips.Add(ipInfo);
        var tokenInfo = new TokenInfo(DefaultToken);
        tokenInfo.UpdateStatus(TokenStatus.Banned, "TestReason");
        _tokenRepository.Tokens.Add(tokenInfo);
        var request = new UserRequest(ipInfo.Id, tokenInfo.Id,DefaultHost, DefaultPath);
        _userRequestRepository.UserRequests.Add(request);
        var validationResult = new AccessValidationResult(true, AccessValidatorReasons.TokenIsBanned);
        _accessValidator.AccessValidationResultToReturn = validationResult;
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserRequest"] = request;
        httpContext.Items["IpInfo"] = ipInfo;
        httpContext.Items["TokenInfo"] = tokenInfo;
        
        _requestValidatorController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _requestValidatorController.CheckRequest() as OkObjectResult;
        var response = result?.Value as AccessValidationResult;

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Block);
        Assert.Equal(validationResult.Reason, response.Reason);
        Assert.Equal(validationResult.Reason, _userRequestRepository.UserRequests[0].DecisionReason);
        Assert.Equal(IpStatus.Banned, _ipRepository.Ips[0].Status);
        Assert.True(_userRequestRepository.UserRequests[0].IsBlocked);
        Assert.Equal(1, _tokenRepository.BanTokenAsyncCount);
        Assert.Equal(1, _ipRepository.BanIpIfNeededAsync2ParamsCount);
    }
}