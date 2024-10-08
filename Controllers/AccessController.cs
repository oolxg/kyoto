using System.Net;
using Kyoto.Exceptions;
using Kyoto.Services.Implementations;
using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/")]
public class AccessController(
    IIpRepository ipRepository,
    ITokenRepository tokenRepository,
    IUserRequestRepository userRequestRepository,
    IRestrictedUrlRepository restrictedUrlRepository) : ControllerBase
{
    [HttpGet("block/ip/{ip}")]
    public async Task<IActionResult> BanIp(string ip, [FromQuery] string reason)
    {
        if (IPAddress.TryParse(ip, out _) == false)
        {
            var response = new
            {
                error = true,
                description = "Invalid IP address",
                ip
            };

            return BadRequest(response);
        }

        var bannedIp = await ipRepository.BanIpIfNeededAsync(ip, reason);

        var requests = await userRequestRepository.FindUserRequestsByIpAsync(ip);
        foreach (var request in requests.Where(request => request.TokenInfo?.Token != null))
            await tokenRepository.BanTokenAsync(request.TokenInfo!.Token, $"Banned along with IP: [{reason}]");

        return Ok(bannedIp);
    }

    [HttpGet("block/token/{token}")]
    public async Task<IActionResult> BanToken(string token, [FromQuery] string reason)
    {
        var bannedToken = await tokenRepository.BanTokenAsync(token, reason);

        var requests = await userRequestRepository.FindUserRequestsByTokenAsync(token);
        foreach (var request in requests)
            await ipRepository.BanIpIfNeededAsync(request.IpInfo.Ip, $"Banned along with token: [{reason}]");

        return Ok(bannedToken);
    }

    [HttpGet("unblock/ip/{ip}")]
    public async Task<IActionResult> UnbanIp(string ip, [FromQuery] string reason)
    {
        if (IPAddress.TryParse(ip, out _) == false)
        {
            var response = new
            {
                error = true,
                description = "Invalid IP address",
                ip
            };

            return BadRequest(response);
        }

        await ipRepository.UnbanIpAsync(ip, reason);

        var requests = await userRequestRepository.FindUserRequestsByIpAsync(ip);

        var tokens = new List<string>();
        foreach (var request in requests.Where(request => request.TokenInfo?.Token != null))
        {
            tokens.Add(request.TokenInfo!.Token);
            await tokenRepository.UnbanTokenAsync(request.TokenInfo!.Token, reason);
        }
        
        var okResponse = new
        {
            message = $"IP {ip} unbanned. Also unbanned tokens, connected with this IP.",
            ip,
            tokens
        };

        return Ok(okResponse);
    }

    [HttpGet("unblock/token/{token}")]
    public async Task<IActionResult> UnbanToken(string token, [FromQuery] string reason)
    {
        await tokenRepository.UnbanTokenAsync(token, reason);

        var requests = await userRequestRepository.FindUserRequestsByTokenAsync(token);
        foreach (var request in requests) 
            await ipRepository.UnbanIpAsync(request.IpInfo.Ip, reason);
        
        var okResponse = new
        {
            message = $"Token {token} unbanned. Also unbanned IPs, connected with this token.",
            token
        };

        return Ok(okResponse);
    }
    
    [HttpGet("toggleHide/ip/{ip}")]
    public async Task<IActionResult> ToggleHideIp(string ip)
    {
        if (IPAddress.TryParse(ip, out _) == false)
        {
            var response = new
            {
                error = true,
                description = "Invalid IP address",
                ip
            };

            return BadRequest(response);
        }

        var ipInfo = await ipRepository.FindIpAsync(ip);
        if (ipInfo == null)
        {
            var response = new
            {
                error = true,
                description = "IP not found",
                ip
            };

            return NotFound(response);
        }
        
        await ipRepository.ChangeShouldHideIfBannedAsync(ip, !ipInfo.ShouldHideIfBanned);
        
        var okResponse = new
        {
            message = $"IP {ip} should hide if banned: {!ipInfo.ShouldHideIfBanned} -> {ipInfo.ShouldHideIfBanned}",
            ip
        };
        
        return Ok(okResponse);
    }
    
    [HttpGet("toggleBlockUrl")]
    public async Task<IActionResult> ToggleBlockUrl(
        [FromQuery] string host,
        [FromQuery] string path,
        [FromQuery] string reason)
    {
        if (!host.EndsWith('/'))
            host += '/';
        
        if (!path.StartsWith('/') && path != "*" && path != "/")
            path = '/' + path;
        
        if (path.EndsWith("/"))
            path = path[..^1];
        
        if (await restrictedUrlRepository.IsUrlBlocked(host, path))
        {
            await restrictedUrlRepository.UnblockUrl(host, path);
            var okResponse = new
            {
                message = $"URL {host}{path} unblocked",
                host,
                path
            };

            return Ok(okResponse);
        }
        
        await restrictedUrlRepository.BlockUrl(host, path, reason);
        var response = new
        {
            message = $"URL {host}{path} blocked",
            host,
            path
        };
        
        return Ok(response);
    }
    
    [HttpGet("whitelist/ip/{ip}")]
    public async Task<IActionResult> WhitelistIp(string ip, [FromQuery] string reason)
    {
        if (IPAddress.TryParse(ip, out _) == false)
        {
            var response = new
            {
                error = true,
                description = "Invalid IP address",
                ip
            };

            return BadRequest(response);
        }

        try
        {
            await ipRepository.WhitelistIpAsync(ip, reason);
        } 
        catch (IpRepositoryException)
        {
            var response = new
            {
                error = true,
                description = "IP is already whitelisted",
                ip
            };

            return BadRequest(response);
        }

        var requests = await userRequestRepository.FindUserRequestsByIpAsync(ip);
        foreach (var request in requests.Where(request => request.TokenInfo?.Token != null))
            await tokenRepository.WhitelistTokenAsync(request.TokenInfo!.Token,
                $"Whitelisted along with IP: [{reason}]");

        var okResponse = new
        {
            message = $"IP {ip} added to white list.",
            ip
        };

        return Ok(okResponse);
    }
}