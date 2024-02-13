using Microsoft.EntityFrameworkCore;
using Smug.Exceptions;
using Tests.Helpers;
using Smug.Models;
using Smug.Models.SmugDbContext;
using Smug.Services.Implementations;

namespace Tests.RepositoryTests;

public class IpRepositoryTests
{
    private readonly SmugDbContext _dbContext;
    private readonly IpRepository _ipRepository;
    private readonly UserRequestRepository _userRequestRepository;
    private readonly TokenRepository _tokenRepository;
    public IpRepositoryTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        _userRequestRepository = new UserRequestRepository(_dbContext);
        _tokenRepository = new TokenRepository(_dbContext, _userRequestRepository);
        _ipRepository = new IpRepository(_dbContext, _userRequestRepository);
    }
    
    ~IpRepositoryTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }

    [Fact]
    public async Task FindOrCreateIpAsync_ShouldSaveNewIp()
    {
        // Arrange
        const string ip = "192.168.0.1";

        // Act
        await _ipRepository.FindOrCreateIpAsync(ip);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
    }

    [Fact]
    public async Task FindOrCreateIpAsync_ShouldNotSaveExistingIp()
    {
        // Arrange
        var ipAddressInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ipAddressInfo.Ip).ToListAsync();

        // Act
        await _ipRepository.FindOrCreateIpAsync(ipAddressInfo.Ip);

        // Assert
        Assert.Single(ips);
        Assert.Equal(ipAddressInfo.Ip, ips[0].Ip);
        Assert.Equal(ipAddressInfo.Id, ips[0].Id);
    }

    [Fact]
    public async Task BanIpAsync_ShouldBanIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.Equal(IpAddressInfo.IpStatus.Banned, ips[0].Status);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpAsync_ShouldBanIpAndHideIt()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpAsync(ip, true, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpAddressInfo.IpStatus.Banned, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.True(ips[0].ShouldHideIfBanned);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpAsync_ShouldAddNewIpIfNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpAsync(ip, reason: reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpAddressInfo.IpStatus.Banned, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpAsync_ShouldHideIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpAsync(ip, true, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpAddressInfo.IpStatus.Banned, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.True(ips[0].ShouldHideIfBanned);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpAsync_ShouldThrowExceptionIfIpAlreadyBanned()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        await _ipRepository.BanIpAsync(ip, reason: reason);
        
        // Act
        try 
        {
            await _ipRepository.BanIpAsync(ip, reason: reason);
            Assert.Fail("BanIpAsync should throw an exception if IpAddressInfo is already banned");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }

    [Fact]
    public async Task BanIpAsync_ShouldThrowExceptionIfIpNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        await _ipRepository.BanIpAsync(ip, reason: reason);

        // Act
        try
        {
            await _ipRepository.BanIpAsync(ip, reason);
            Assert.Fail("BanIpAsync should throw an exception if IpAddressInfo is not in the database");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }

    [Fact]
    public async Task UnbanIpAsync_ShouldUnbanIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        await _ipRepository.BanIpAsync(ip, reason);

        // Act
        await _ipRepository.UnbanIpAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpAddressInfo.IpStatus.Normal, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task UnbanIpAsync_ShouldNotUnbanIpIfNotBanned()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        try
        {
            await _ipRepository.UnbanIpAsync(ip, reason);
            Assert.Fail("UnbanIpAsync should throw an exception if IpAddressInfo is not in the database");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }

    [Fact]
    public async Task FindIpAsync_ShouldFindIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        await _ipRepository.FindOrCreateIpAsync(ip);

        // Act
        var foundIp = await _ipRepository.FindIpAsync(ip);

        // Assert
        Assert.NotNull(foundIp);
        Assert.Equal(ip, foundIp.Ip);
    }

    [Fact]
    public async Task FindIpAsync_ShouldReturnNullIfIpNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";

        // Act
        var foundIp = await _ipRepository.FindIpAsync(ip);

        // Assert
        Assert.Null(foundIp);
    }

    [Fact]
    public async Task FindIpAsync_ShouldFindIpByIpId()
    {
        // Arrange
        const string ip = "192.168.0.1";
        var savedIp = await _ipRepository.FindOrCreateIpAsync(ip);

        // Act
        var foundIp = await _ipRepository.FindIpAsync(savedIp.Id);

        // Assert
        Assert.NotNull(foundIp);
        Assert.Equal(ip, foundIp.Ip);
    }

    [Fact]
    public async Task FindIpAsync_ShouldReturnNullIfIpNotFoundByIpId()
    {
        // Arrange

        // Act
        var foundIp = await _ipRepository.FindIpAsync(Guid.NewGuid());

        // Assert
        Assert.Null(foundIp);
    }

    [Fact]

    public async Task ChangeShouldHideIfBannedAsync_ShouldChangeShouldHideStatus()
    {
        // Arrange
        const string ip = "192.168.0.1";
        await _ipRepository.FindOrCreateIpAsync(ip);

        // Act
        await _ipRepository.ChangeShouldHideIfBannedAsync(ip, true);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.True(ips[0].ShouldHideIfBanned);
    }

    [Fact]
    public async Task WhitelistIpAsync_ShouldWhitelistIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.WhitelistIpAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpAddressInfo.IpStatus.Whitelisted, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task WhitelistIpAsync_ShouldAddNewIpIfNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        
        // Act
        await _ipRepository.WhitelistIpAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();
        
        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpAddressInfo.IpStatus.Whitelisted, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task WhitelistIpAsync_ShouldThrowExceptionIfIpAlreadyWhitelisted()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        await _ipRepository.WhitelistIpAsync(ip, reason);

        // Act
        try
        {
            await _ipRepository.WhitelistIpAsync(ip, reason);
            Assert.Fail("WhitelistIpAsync should throw an exception if IpAddressInfo is already whitelisted");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }

    [Fact]
    public async Task AddIpAddressesAsync_ShouldAddIpAddresses()
    {
        // Arrange
        var ipInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");
        var tokens = new List<TokenInfo>
        {
            new("testToken-1"),
            new("testToken-2"),
            new("testToken-3"),
        };
        await _dbContext.Tokens.AddRangeAsync(tokens);
        
        // Act
        await _ipRepository.AddIpAddressesAsync(ipInfo.Ip, tokens.Select(token => token.Id));
        var ipTokens = await _dbContext.IpTokens.Where(ipToken => ipToken.IpId == ipInfo.Id).ToListAsync();
        
        // Assert
        Assert.Equal(tokens.Count, ipTokens.Count);
        foreach (var ipToken in ipTokens)
        {
            Assert.Contains(tokens, token => token.Id == ipToken.TokenId);
        }
    }

    [Fact]
    public async Task AddIpAddressesAsync_ShouldThrowExceptionIfIpNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        var tokenIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        // Act
        try
        {
            await _ipRepository.AddIpAddressesAsync(ip, tokenIds);
            Assert.Fail("AddIpAddressesAsync should throw an exception if IpAddressInfo is not in the database");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }

    [Fact]
    public async Task AddUserRequestToIpAsync_ShouldAddUserRequestToIp()
    {
        // Arrange
        var ipAddressInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");
        var userRequest = new UserRequest(ipAddressInfo.Id, null, "example.com", "/test-path/");
        await _userRequestRepository.SaveUserRequestAsync(userRequest);
        
        // Act
        await _ipRepository.AddUserRequestToIpAsync(ipAddressInfo.Ip, userRequest.Id);
        var userRequests = await _dbContext.UserRequests.Where(ur => ur.IpInfoId == ipAddressInfo.Id).ToListAsync();
        
        // Assert
        Assert.Single(userRequests);
        Assert.Equal(userRequest.Id, userRequests[0].Id);
    }

    [Fact]
    public async Task AddUserRequestToIpAsync_ShouldThrowExceptionIfIpNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        var userRequest = new UserRequest(Guid.NewGuid(), null, "example.com", "/test-path/");

        // Act
        try
        {
            await _ipRepository.AddUserRequestToIpAsync(ip, userRequest.Id);
            Assert.Fail("AddUserRequestToIpAsync should throw an exception if IpAddressInfo is not in the database");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }

    [Fact]
    public async Task AddUserRequestToIpAsync_ShouldThrowExceptionIfUserRequestNotFound()
    {
        // Arrange
        var ipAddressInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");

        // Act
        try
        {
            await _ipRepository.AddUserRequestToIpAsync(ipAddressInfo.Ip, Guid.NewGuid());
            Assert.Fail("AddUserRequestToIpAsync should throw an exception if UserRequest is not in the database");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
    }
}