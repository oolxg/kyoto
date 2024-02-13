using Microsoft.EntityFrameworkCore;
using Smug.Exceptions;
using Tests.Helpers;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Implementations;

namespace Tests.RepositoryTests;

public class TokenRepositoryTests
{
    private readonly SmugDbContext _dbContext;
    private readonly TokenRepository _tokenRepository;
    private readonly IpRepository _ipRepository;
    
    public TokenRepositoryTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        var userRequestRepository = new UserRequestRepository(_dbContext);
        _tokenRepository = new TokenRepository(_dbContext, userRequestRepository);
        _ipRepository = new IpRepository(_dbContext, userRequestRepository);
    }
    
    ~TokenRepositoryTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }
    
    [Fact]
    public async Task FindOrCreateTokenAsync_ShouldSaveNewToken()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        
        // Act
        var tokenInfo = await _tokenRepository.FindOrCreateTokenAsync(token);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Normal, tokens[0].Status);
        Assert.Null(tokens[0].Reason);
        Assert.Equal(tokenInfo.Id, tokens[0].Id);
    }
    
    [Fact]
    public async Task FindOrCreateTokenAsync_ShouldNotSaveExistingToken()
    {
        // Arrange
        var tokenInfo = await _tokenRepository.FindOrCreateTokenAsync("test-token");
        var tokens = await _dbContext.Tokens.Where(t => t.Token == tokenInfo.Token).ToListAsync();
        
        // Act
        await _tokenRepository.FindOrCreateTokenAsync(tokenInfo.Token);
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(tokenInfo.Token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Normal, tokens[0].Status);
        Assert.Null(tokens[0].Reason);
        Assert.Equal(tokenInfo.Id, tokens[0].Id);
    }
    
    [Fact]
    public async Task BanTokenAsync_ShouldBanToken()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        const string reason = "test reason";
        
        // Act
        await _tokenRepository.BanTokenAsync(token, reason);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Banned, tokens[0].Status);
        Assert.Equal(reason, tokens[0].Reason);
    }
    
    [Fact]
    public async Task BanTokenAsync_ShouldUpdateReason()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        const string reason = "test reason";
        
        // Act
        await _tokenRepository.BanTokenAsync(token, reason);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Banned, tokens[0].Status);
        Assert.Equal(reason, tokens[0].Reason);
    }
    
    [Fact]
    public async Task BanTokenAsync_ShouldNotBanTokenIfAlreadyBanned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        const string reason = "test reason";
        await _tokenRepository.BanTokenAsync(token, reason);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Act
        await _tokenRepository.BanTokenAsync(token, reason);
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Banned, tokens[0].Status);
        Assert.Equal(reason, tokens[0].Reason);
    }
    
    [Fact]
    public async Task UnbanTokenAsync_ShouldUnbanToken()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var reason = "test reason";
        await _tokenRepository.BanTokenAsync(token, reason);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Act
        reason = "another reason";
        await _tokenRepository.UnbanTokenAsync(token, reason);
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Normal, tokens[0].Status);
        Assert.Equal(reason, tokens[0].Reason);
    }
    
    [Fact]
    public async Task UnbanTokenAsync_ShouldUpdateReason()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        const string reason = "test reason";
        await _tokenRepository.BanTokenAsync(token, reason);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Act
        await _tokenRepository.UnbanTokenAsync(token, reason);
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Normal, tokens[0].Status);
        Assert.Equal(reason, tokens[0].Reason);
    }
    
    [Fact]
    public async Task UnbanTokenAsync_ShouldNotUnbanTokenIfAlreadyUnbanned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        const string reason = "test reason";
        await _tokenRepository.BanTokenAsync(token, reason);
        var tokens = await _dbContext.Tokens.Where(t => t.Token == token).ToListAsync();
        
        // Act
        await _tokenRepository.UnbanTokenAsync(token, reason);
        
        // Assert
        Assert.Single(tokens);
        Assert.Equal(token, tokens[0].Token);
        Assert.Equal(TokenInfo.TokenStatus.Normal, tokens[0].Status);
        Assert.Equal(reason, tokens[0].Reason);
    }
    
    [Fact]
    public async Task FindTokenAsync_ShouldFindTokenByToken()
    {
        // Arrange
        var tokens = MockTokens();
        await _dbContext.Tokens.AddRangeAsync(tokens);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var token = await _tokenRepository.FindTokenAsync(tokens[1].Token);
        
        // Assert
        Assert.Equal(tokens[1], token);
    }
    
    [Fact]
    public async Task FindTokenAsync_ShouldFindTokenById()
    {
        // Arrange
        var tokens = MockTokens();
        await _dbContext.Tokens.AddRangeAsync(tokens);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var token = await _tokenRepository.FindTokenAsync(tokens[1].Id);
        
        // Assert
        Assert.Equal(tokens[1], token);
    }
    
    [Fact]
    public async Task FindTokenAsync_ShouldReturnNullIfTokenNotFoundByToken()
    {
        // Arrange
        var tokens = MockTokens();
        await _dbContext.Tokens.AddRangeAsync(tokens);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var token = await _tokenRepository.FindTokenAsync(Guid.NewGuid().ToString());
        
        // Assert
        Assert.Null(token);
    }
    
    [Fact]
    public async Task FindTokenAsync_ShouldReturnNullIfTokenNotFoundById()
    {
        // Arrange
        var tokens = MockTokens();
        await _dbContext.Tokens.AddRangeAsync(tokens);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var token = await _tokenRepository.FindTokenAsync(Guid.NewGuid());
        
        // Assert
        Assert.Null(token);
    }
    
    [Fact]
    public async Task AddIpAddressesAsync_ShouldAddIpAddresses()
    {
        // Arrange
        var token = MockToken();
        var ipAddresses = new List<IpAddressInfo>
        {
            new("192.168.0.1"),
            new("192.168.0.2"),
            new("192.168.0.3")
        };
        
        await _dbContext.Tokens.AddAsync(token);
        await _dbContext.Ips.AddRangeAsync(ipAddresses);
        await _dbContext.SaveChangesAsync();
        
        // Act
        await _tokenRepository.AddIpAddressesAsync(token.Token, ipAddresses.Select(ip => ip.Id).ToList());
        var tokenIps = await _dbContext.IpTokens.Where(ipToken => ipToken.TokenId == token.Id).ToListAsync();
        
        // Assert
        Assert.Equal(ipAddresses.Count, tokenIps.Count);
        foreach (var tokenIp in tokenIps)
        {
            Assert.Contains(ipAddresses, ip => ip.Id == tokenIp.IpId);
        }
    }

    [Fact]
    public async Task AddIpAddressesAsync_ShouldNotAddIpAddressesIfTokenNotFound()
    {
        // Arrange
        var token = MockToken();
        var ipAddresses = new List<IpAddressInfo>
        {
            new("192.168.0.1"),
            new("192.168.0.2"),
            new("192.168.0.3")
        };
        await _dbContext.Ips.AddRangeAsync(ipAddresses);
        await _dbContext.SaveChangesAsync();
        await _tokenRepository.FindOrCreateTokenAsync(token.Token);

        // Act
        await _tokenRepository.AddIpAddressesAsync(token.Token, ipAddresses.Select(ip => ip.Id).ToList());
        var tokenIps = await _dbContext.IpTokens.Where(ipToken => ipToken.TokenId == token.Id).ToListAsync();

        // Assert
        Assert.Empty(tokenIps);
    }
    
    [Fact]
    public async Task AddUserRequestToTokenAsync_ShouldAddUserRequest()
    {
        // Arrange
        var token = MockToken();
        const string ip = "192.168.0.1";
        var ipInfo = await _ipRepository.FindOrCreateIpAsync(ip);
        var userRequest = new UserRequest(ipInfo.Id, null, "example.com", "/test-path/");
        await _dbContext.Tokens.AddAsync(token);
        await _dbContext.UserRequests.AddAsync(userRequest);
        await _dbContext.SaveChangesAsync();
        
        // Act
        await _tokenRepository.AddUserRequestToTokenAsync(token.Token, userRequest.Id);
        var tokenUserRequests = await _dbContext.UserRequests.Where(ur => ur.TokenInfoId == token.Id).ToListAsync();
        
        // Assert
        Assert.Single(tokenUserRequests);
        Assert.Equal(userRequest.Id, tokenUserRequests[0].Id);
    }

    [Fact]
    public async Task AddUserRequestToTokenAsync_ShouldNotAddUserRequestIfTokenNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        
        var ipInfo = await _ipRepository.FindOrCreateIpAsync(ip);
        var userRequest = new UserRequest(ipInfo.Id, null, "example.com", "/test-path/");
        await _dbContext.UserRequests.AddAsync(userRequest);
        await _dbContext.SaveChangesAsync();
        
        // Act
        try
        {
            await _tokenRepository.AddUserRequestToTokenAsync("NotExistingToken", userRequest.Id);
        }
        catch (TokenRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }
    
    private static TokenInfo MockToken(string? token = null)
    {
        return new TokenInfo(token ?? Guid.NewGuid().ToString());
    }
    
    private static List<TokenInfo> MockTokens()
    {
        var tokens = new List<TokenInfo>();
        
        for (var i = 0; i < 3; i++)
        {
            tokens.Add(MockToken($"testToken-{i}"));
        }
        
        return tokens;
    }
}