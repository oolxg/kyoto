using System.Text;
using Kyoto.Middlewares;
using Kyoto.Models;
using Newtonsoft.Json;
using Kyoto.Tests.Fakes;
using Microsoft.AspNetCore.Http;

namespace Kyoto.Tests.MiddlewareTests;

public class RequestSaverMiddlewareTests
{
    private readonly RequestDelegate _next = (_) => Task.CompletedTask;
    private readonly IpRepositoryFake _ipRepositoryFake;
    private readonly TokenRepositoryFake _tokenRepositoryFake;
    private readonly UserRequestRepositoryFake _userRequestRepositoryFake;
    private readonly RequestSaverMiddleware _requestSaverMiddleware;
    private const string DefaultIp = "192.168.0.1";
    private const string DefaultHost = "example.com";
    private const string DefaultPath = "/test/path.";

    public RequestSaverMiddlewareTests()
    {
        _userRequestRepositoryFake = new UserRequestRepositoryFake();
        _tokenRepositoryFake = new TokenRepositoryFake(_userRequestRepositoryFake);
        _ipRepositoryFake = new IpRepositoryFake(_userRequestRepositoryFake, _tokenRepositoryFake);
        _requestSaverMiddleware = new RequestSaverMiddleware(_next);
    }


    [Fact]
    public async Task InvokeAsync_GivenBadData_ShouldNotSaveUserRequestAndReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        const string requestBody = "{\"Invalid\": \"Data\"}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Null(context.Items["UserRequest"]);
        Assert.Null(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
    }

    [Fact]
    public async Task InvokeAsync_GivenValidDataAndNoToken_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo();
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(1, _userRequestRepositoryFake.SaveUserRequestAsyncCount);
        Assert.Single(_ipRepositoryFake.Ips);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
        Assert.Empty(_tokenRepositoryFake.Tokens);
    }

    [Fact]
    public async Task InvokeAsync_GivenValidDataAndToken_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo(token: "test_token");
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.NotNull(context.Items["TokenInfo"]);
        Assert.Equal(context.Items["TokenInfo"], _tokenRepositoryFake.Tokens[0]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenValidDataAndTokenAndHostWithHttp_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo(host: "http://example.com");
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
        Assert.Equal("example.com/", _userRequestRepositoryFake.UserRequests[0].Host);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenValidDataAndTokenAndHostWithHttps_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo(host: "https://example.com");
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
        Assert.Equal("example.com/", _userRequestRepositoryFake.UserRequests[0].Host);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenValidDataAndTokenAndPathWithoutSlash_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo(path: "test/path");
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
        Assert.Equal("/test/path", _userRequestRepositoryFake.UserRequests[0].Path);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenMissingHost_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = "{" +
                          "\"RequestDate\": \"2022-01-01T00:00:00\"," +
                          "\"UserIp\": \"127.0.0.1\"," +
                          "\"Path\": \"/test/path\"," +
                          "\"Token\": \"test-token\"," +
                          "\"Headers\": {" +
                          "\"User-Agent\": \"TestAgent\"" +
                          "}" +
                          "}";
        
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestInfo));
        
        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);
        
        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Null(context.Items["UserRequest"]);
        Assert.Null(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenMissingPath_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = "{" +
                          "\"RequestDate\": \"2022-01-01T00:00:00\"," +
                          "\"UserIp\": \"127.0.0.1\"," +
                          "\"Host\": \"example.com\"," +
                          "\"Token\": \"test-token\"," +
                          "\"Headers\": {" +
                          "\"User-Agent\": \"TestAgent\"" +
                          "}" +
                          "}";
        
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestInfo));
        
        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);
        
        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Null(context.Items["UserRequest"]);
        Assert.Null(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenMissingIp_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = "{" +
                          "\"RequestDate\": \"2022-01-01T00:00:00\"," +
                          "\"Host\": \"example.com\"," +
                          "\"Path\": \"/test/path\"," +
                          "\"Token\": \"test-token\"," +
                          "\"Headers\": {" +
                          "\"User-Agent\": \"TestAgent\"" +
                          "}" +
                          "}";
        
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestInfo));
        
        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);
        
        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Null(context.Items["UserRequest"]);
        Assert.Null(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenMissingRequestDate_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = "{" +
                          "\"UserIp\": \"127.0.0.1\"," +
                          "\"Host\": \"example.com\"," +
                          "\"Path\": \"/test/path\"," +
                          "\"Token\": \"test-token\"," +
                          "\"Headers\": {" +
                          "\"User-Agent\": \"TestAgent\"" +
                          "}" +
                          "}";
        
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestInfo));
        
        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);
        
        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Null(context.Items["UserRequest"]);
        Assert.Null(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenMissingToken_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo(token: null);
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
    }
    
    [Fact]
    public async Task InvokeAsync_GivenMissingHeaders_ShouldSaveUserRequestAndReturn200()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var requestInfo = CreateRequestInfo(headers: null);
        var requestBody = JsonConvert.SerializeObject(requestInfo);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        // Act
        await _requestSaverMiddleware.InvokeAsync(context, _ipRepositoryFake, _tokenRepositoryFake,
            _userRequestRepositoryFake);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(context.Items["UserRequest"]);
        Assert.NotNull(context.Items["IpInfo"]);
        Assert.Null(context.Items["TokenInfo"]);
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
        Assert.Empty(_userRequestRepositoryFake.UserRequests[0].Headers);
    }

    private static UserRequestInfo CreateRequestInfo(
        DateTime requestDate = default,
        string ip = DefaultIp,
        string host = DefaultHost,
        string path = DefaultPath,
        string? token = null,
        Dictionary<string, string>? headers = null)
    {
        return new UserRequestInfo
        {
            RequestDate = requestDate,
            UserIp = ip,
            Host = host,
            Path = path,
            Token = token,
            Headers = headers
        };
    }
}