using System.Text;
using Kyoto.Middlewares;
using Kyoto.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Kyoto.Tests.Fakes;

namespace Kyoto.Tests.MiddlewareTests;

public class RequestSaverMiddlewareTests
{
    private readonly RequestDelegate _next = (context) => Task.CompletedTask;
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
        var requestInfo = CreateRequestInfo(headers: new Dictionary<string, string> { { "Token", "test-token" } });
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
        Assert.Single(_userRequestRepositoryFake.UserRequests);
        Assert.Equal(requestInfo.UserIp, _ipRepositoryFake.Ips[0].Ip);
    }

    private static UserRequestInfo CreateRequestInfo(
        DateTime requestDate = default,
        string ip = DefaultIp,
        string host = DefaultHost,
        string path = DefaultPath,
        Dictionary<string, string>? headers = null)
    {
        return new UserRequestInfo
        {
            RequestDate = requestDate,
            UserIp = ip,
            Host = host,
            Path = path,
            Headers = headers
        };
    }
}