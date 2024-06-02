using Kyoto.Exceptions;
using Tests.Helpers;
using Kyoto.Models;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Tests.RepositoryTests;

public class IpRepositoryTests
{
    private readonly KyotoDbContext _dbContext;
    private readonly IpRepository _ipRepository;
    private readonly UserRequestRepository _userRequestRepository;

    public IpRepositoryTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        _userRequestRepository = new UserRequestRepository(_dbContext);
        _ipRepository = new IpRepository(_dbContext, _userRequestRepository);
    }

    ~IpRepositoryTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }

    [Fact]
    public async Task FindOrCreateIpAsync_GivenIp_ShouldSaveNewIp()
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
    public async Task BanIpIfNeededAsync_GivenReason_ShouldBanIpWithReason()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpIfNeededAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.Equal(IpStatus.Banned, ips[0].Status);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpIfNeededAsync_ShouldAddNewIpIfNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpIfNeededAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpStatus.Banned, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpIfNeededAsync_ShouldIgnoreIfAlreadyBanned()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        await _ipRepository.BanIpIfNeededAsync(ip, reason);

        // Act
        await _ipRepository.BanIpIfNeededAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpStatus.Banned, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task BanIpIfNeededAsync_ShouldAddNewIpAndBanIt()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";

        // Act
        await _ipRepository.BanIpIfNeededAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpStatus.Banned, ips[0].Status);
        Assert.Equal(reason, ips[0].StatusChangeReason);
        Assert.NotNull(ips[0].StatusChangeDate);
    }

    [Fact]
    public async Task UnbanIpAsync_ShouldUnbanIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        const string reason = "test reason";
        await _ipRepository.BanIpIfNeededAsync(ip, reason);

        // Act
        await _ipRepository.UnbanIpAsync(ip, reason);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(IpStatus.Normal, ips[0].Status);
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
        var foundIp = await _ipRepository.FindIpAsync(savedIp.Ip);

        // Assert
        Assert.NotNull(foundIp);
        Assert.Equal(ip, foundIp.Ip);
    }

    [Fact]
    public async Task FindIpAsync_ShouldReturnNullIfIpNotFoundByIpId()
    {
        // Arrange

        // Act
        var foundIp = await _ipRepository.FindIpAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(foundIp);
    }
    
    [Fact]
    public async Task FindIpAsync_byId_ShouldFindIpByIpId()
    {
        // Arrange
        var ip = new IpAddressInfo("192.168.0.1");
        await _dbContext.Ips.AddAsync(ip);
        
        // Act
        var foundIp = await _ipRepository.FindIpAsync(ip.Id);
        
        // Assert
        Assert.NotNull(foundIp);
        Assert.Equal(ip.Ip, foundIp.Ip);
    }
    
    [Fact]
    public async Task FindIpAsync_byId_ShouldReturnNullIfIpNotFoundByIpId()
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
        Assert.Equal(IpStatus.Whitelisted, ips[0].Status);
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
        Assert.Equal(IpStatus.Whitelisted, ips[0].Status);
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
    public async Task AddTokensAsync_ShouldAddTokens()
    {
        // Arrange
        var ipInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");
        var token = new TokenInfo("testToken");
        await _dbContext.Tokens.AddAsync(token);

        // Act
        await _ipRepository.AddTokenAsyncIfNeeded(ipInfo.Ip, token.Id);
        var ipTokens = await _dbContext.IpTokens.Where(it => it.IpAddressInfoId == ipInfo.Id).ToListAsync();

        // Assert
        Assert.Single(ipTokens);
        Assert.Equal(ipInfo.Id, ipTokens[0].IpAddressInfoId);
        Assert.Equal(token.Id, ipTokens[0].TokenInfoId);
    }

    [Fact]
    public async Task AddTokensAsync_ShouldThrowExceptionIfTokenNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";
        var nonExistentTokenId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<IpRepositoryException>(() =>
            _ipRepository.AddTokenAsyncIfNeeded(ip, nonExistentTokenId));
    }

    [Fact]
    public async Task AddTokensAsync_ShouldThrowExceptionIfIpNotFound()
    {
        // Arrange
        var tokenInfo = new TokenInfo("testToken");
        const string nonExistentIp = "192.168.0.1";
        await _dbContext.Tokens.AddAsync(tokenInfo);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<IpRepositoryException>(() =>
            _ipRepository.AddTokenAsyncIfNeeded(nonExistentIp, tokenInfo.Id));
    }

    [Fact]
    public async Task AddTokensAsync_ShouldThrowExceptionIfIpAndTokenNotFound()
    {
        // Arrange
        var nonExistingToken = new TokenInfo("testToken");
        const string nonExistentIp = "192.168.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<IpRepositoryException>(() =>
            _ipRepository.AddTokenAsyncIfNeeded(nonExistentIp, nonExistingToken.Id));
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

        // Act & Assert
        await Assert.ThrowsAsync<IpRepositoryException>(() =>
            _ipRepository.AddUserRequestToIpAsync(ip, userRequest.Id));
    }

    [Fact]
    public async Task AddUserRequestToIpAsync_ShouldThrowExceptionIfUserRequestNotFound()
    {
        // Arrange
        var ipAddressInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");

        // Act & Assert
        await Assert.ThrowsAsync<IpRepositoryException>(() =>
            _ipRepository.AddUserRequestToIpAsync(ipAddressInfo.Ip, Guid.NewGuid()));
    }

    [Fact]

    public async Task FundTokenByIpAsync_ShouldFindTokensByIp()
    {
        // Arrange
        var ipAddressInfo = await _ipRepository.FindOrCreateIpAsync("192.168.0.1");
        var token = new TokenInfo("testToken");
        await _dbContext.Tokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
        await _ipRepository.AddTokenAsyncIfNeeded(ipAddressInfo.Ip, token.Id);

        // Act
        var tokens = await _ipRepository.FindTokensByIpAsync(ipAddressInfo.Ip);

        // Assert
        Assert.Single(tokens);
        Assert.Equal(token.Id, tokens[0].Id);
    }

    [Fact]
    public async Task FundTokenByIpAsync_ShouldThrowExceptionIfIpNotFound()
    {
        // Arrange
        const string nonExistentIp = "192.168.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<IpRepositoryException>(() =>
            _ipRepository.FindTokensByIpAsync(nonExistentIp));
    }
}