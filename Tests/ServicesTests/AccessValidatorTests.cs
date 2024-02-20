using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Implementations;
using Smug.Tests.Fakes;
using Tests.Helpers;

namespace Tests.RepositoryTests;

public class AccessValidatorTests
{
    private readonly SmugDbContext _dbContext;
    private readonly IpRepositoryFake _ipRepositoryFake;
    private readonly TokenRepositoryFake _tokenRepositoryFake;
    private readonly UserRequestRepositoryFake _userRequestRepositoryFake;
    private readonly RestrictedUrlRepositoryFake _restrictedUrlRepositoryFake;
    private readonly AccessValidator _accessValidator;
    private Dictionary<string, string> _defaultHeaders = new()
    {
        {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36"}
    };
    
    public AccessValidatorTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        _userRequestRepositoryFake = new UserRequestRepositoryFake();
        _tokenRepositoryFake = new TokenRepositoryFake(_userRequestRepositoryFake);
        _ipRepositoryFake = new IpRepositoryFake(_userRequestRepositoryFake);
        _restrictedUrlRepositoryFake = new RestrictedUrlRepositoryFake();
        
        _accessValidator = new AccessValidator(
            _userRequestRepositoryFake,
            _tokenRepositoryFake, 
            _ipRepositoryFake, 
            _restrictedUrlRepositoryFake);
    }
    
    ~AccessValidatorTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }

    // Simple tests, either IP or token is banned, or both are not banned
    [Fact]
    public async Task ValidateRequestAsync_WhenIpIsNotBannedAndNoTokenGiven_ReturnsValidationResultWithBlockFalse_ShouldNotBanTokenOrIp()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.False(result.Block);
        Assert.Equal("Request is valid", result.Reason);
        Assert.Equal(0, _ipRepositoryFake.BanIpIfNeededAsync3ParamsCount);
        // Token is not given, so it cant be saved
        Assert.Equal(0, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(0, _tokenRepositoryFake.BanTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }

    [Fact]
    public async Task ValidateRequestAsync_WhenIpIsNotBanned_TokenGiven_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.False(result.Block);
        Assert.Equal("Request is valid", result.Reason);
        Assert.Equal(0, _ipRepositoryFake.BanIpIfNeededAsync3ParamsCount);
        Assert.Equal(0, _tokenRepositoryFake.BanTokenAsyncCount);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenIpIsBannedAndNoTokenGiven_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Banned, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is banned", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(0, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(0, _tokenRepositoryFake.BanTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenTokenIsBanned_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Banned, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Token is banned", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenIpIsNotBanned_GivenIpAndNoToken_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is valid", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenIpAndTokenAreNotBanned_GivenIpAndToken_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is valid", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenIpIsBannedAndTokenNotBanned_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Banned, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is banned", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenIpNotBannedAndTokenBanned_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Banned, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Token is banned", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    // Tests for whitelisted IPs and tokens
    [Fact]
    public async Task ValidateRequestAsync_WhenIpIsWhitelisted_TokenNotGiven_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(0, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenTokenIsWhitelisted_GivenWhitelistedToken_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Token is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenIpAndTokenAreWhitelisted_GivenWhitelistedIpAndToken_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    // Tests for blocked URLs
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithNoTimeLimit_NoTokenGiven_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason"));
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Requested URL is blocked", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithNoTimeLimit_TokenGiven_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason"));
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Requested URL is blocked", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithNoTimeLimit_GivenWhitelistedIpAndToken_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason"));
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithNoTimeLimit_GivenWhitelistedIpAndNoToken_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason"));
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithNoTimeLimit_GivenWhitelistedTokenAndIp_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason"));
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Token is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenNoTokenAndDateIsNotPassed_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Requested URL is blocked", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenTokenAndDateNotIsPassed_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Requested URL is blocked", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenNoTokenAndDateIsPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(-1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is valid", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenTokenAndDateIsPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(-1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is valid", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenWhitelistedIpAndTokenAndDateIsNotPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenWhitelistedIpAndTokenAndDateIsPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        ipInfo.UpdateStatus(IpStatus.Whitelisted, "Test reason");
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(-1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("IP is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenWhitelistedTokenAndIpAndDateIsNotPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Token is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenUrlIsBlockedWithTimeLimit_GivenWhitelistedTokenAndIpAndDateIsPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        var tokenInfo = new TokenInfo("Test token");
        tokenInfo.UpdateStatus(TokenStatus.Whitelisted, "Test reason");
        _tokenRepositoryFake.Tokens.Add(tokenInfo);
        
        _defaultHeaders.Add("Token", tokenInfo.Token);
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        _ipRepositoryFake.Ips.Add(ipInfo);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, tokenInfo.Id);
        userRequest.IpInfo = ipInfo;
        userRequest.TokenInfo = tokenInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(-1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Token is whitelisted", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _tokenRepositoryFake.FindOrCreateTokenAsyncCount);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    // Tests for crawlers
    [Fact]
    public async Task ValidateRequestAsync_WhenRequestIsFromCrawler_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        _defaultHeaders["User-Agent"] = "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)";
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is from a crawler like Yandex or Google bot", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenRequestIsFromCrawler_GivenBlockedUrlWithNoTimeLimit_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        _defaultHeaders["User-Agent"] = "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)";
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason"));
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is from a crawler like Yandex or Google bot", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenRequestIsFromCrawler_GivenBlockedUrlWithTimeLimitAndDateIsNotPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        _defaultHeaders["User-Agent"] = "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)";
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is from a crawler like Yandex or Google bot", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_WhenRequestIsFromCrawler_GivenBlockedUrlWithTimeLimitAndDateIsPassed_ReturnsValidationResultWithBlockFalse()
    {
        // Arrange
        _defaultHeaders["User-Agent"] = "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)";
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        var restrictedUrl = new RestrictedUrl(userRequestInfo.Host, userRequestInfo.Path,"Test reason", DateTime.UtcNow.AddDays(-1));
        _restrictedUrlRepositoryFake.RestrictedUrls.Add(restrictedUrl);
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Request is from a crawler like Yandex or Google bot", result.Reason);
        Assert.False(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    // Tests for User-Agent validation  
    [Fact]
    public async Task ValidateRequestAsync_GivenNoUserAgent_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        var userRequestInfo = CreateRequestInfo(headers: new Dictionary<string, string>());
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("User-Agent is empty", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    [Fact]
    public async Task ValidateRequestAsync_GivenUserAgentWithPython_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        _defaultHeaders["User-Agent"] = "Python/3.9";
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("User-Agent contains `python`, seems like a bot", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    
    // Tests for referrer validation
    [Fact]
    public async Task ValidateRequestAsync_GivenRefererWithJsRedir_ReturnsValidationResultWithBlockTrue()
    {
        // Arrange
        _defaultHeaders.Add("Referer", "http://yandex.ru/clck/jsredir?from=yandex.ru%3Bsearch");
        var userRequestInfo = CreateRequestInfo(headers: _defaultHeaders);
        var ipInfo = new IpAddressInfo(userRequestInfo.UserIp);
        var userRequest = userRequestInfo.AsUserRequest(ipInfo.Id, null);
        userRequest.IpInfo = ipInfo;
        
        // Act
        var result = await _accessValidator.ValidateAsync(userRequest);
        
        // Assert
        Assert.Equal("Referer contains `jsredir`, seems like a bot", result.Reason);
        Assert.True(result.Block);
        Assert.Equal(1, _ipRepositoryFake.FindOrCreateIpAsyncCount);
    }
    

    private static UserRequestInfo CreateRequestInfo(
        DateTime requestDate = default,
        string ip = "192.168.0.1",
        string host = "example.com",
        string path = "/test/path/",
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