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
    
    public IpRepositoryTests()
    {
        _dbContext = DbContextFactory.CreateDbContext();
        _ipRepository = new IpRepository(_dbContext);
    }
    
    ~IpRepositoryTests()
    {
        DbContextFactory.DisposeDbContext(_dbContext);
    }

    [Fact]
    public async Task SaveIpAsync_ShouldSaveNewIp()
    {
        // Arrange
        const string ip = "192.168.0.1";

        // Act
        await _ipRepository.SaveIpIfNeededAsync(ip);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
    }

    [Fact]
    public async Task SaveIpAsync_ShouldNotSaveExistingIp()
    {
        // Arrange
        const string ip = "192.168.0.1";
        await _ipRepository.SaveIpIfNeededAsync(ip);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Act
        await _ipRepository.SaveIpIfNeededAsync(ip);

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
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
        await _ipRepository.SaveIpIfNeededAsync(ip);

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
        var savedIp = await _ipRepository.SaveIpIfNeededAsync(ip);

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

    public async Task ChangeShouldHideIfBannedAsync_ShouldChangeShouldHideIfBanned()
    {
        // Arrange
        const string ip = "192.168.0.1";
        await _ipRepository.SaveIpIfNeededAsync(ip);

        // Act
        await _ipRepository.ChangeShouldHideIfBannedAsync(ip, true);
        var ips = await _dbContext.Ips.Where(ipInfo => ipInfo.Ip == ip).ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.True(ips[0].ShouldHideIfBanned);
    }

    [Fact]
    public async Task ChangeShouldHideIfBannedAsync_ShouldThrowExceptionIfIpNotFound()
    {
        // Arrange
        const string ip = "192.168.0.1";

        // Act
        try
        {
            await _ipRepository.ChangeShouldHideIfBannedAsync(ip, true);
            Assert.Fail("ChangeShouldHideIfBannedAsync should throw an exception if IpAddressInfo is not in the database");
        }
        catch (IpRepositoryException)
        {
            // Assert
            Assert.True(true);
        }
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
        const string ip = "192.168.0.1";
        var tokenIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        await _ipRepository.SaveIpIfNeededAsync(ip);

        // Act
        await _ipRepository.AddIpAddressesAsync(ip, tokenIds);
        var ips = await _dbContext.Ips
            .Include(ipInfo => ipInfo.IpTokens)
            .Where(ipInfo => ipInfo.Ip == ip)
            .ToListAsync();

        // Assert
        Assert.Single(ips);
        Assert.Equal(ip, ips[0].Ip);
        Assert.Equal(tokenIds.Count, ips[0].IpTokens.Count);
        Assert.Equal(tokenIds[0], ips[0].IpTokens[0].TokenId);
        Assert.Equal(tokenIds[1], ips[0].IpTokens[1].TokenId);
        Assert.Equal(tokenIds[2], ips[0].IpTokens[2].TokenId);
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
}