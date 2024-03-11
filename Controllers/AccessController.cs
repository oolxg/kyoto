using System.Net;
using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/")]
public class AccessController(
    IIpRepository ipRepository,
    ITokenRepository tokenRepository,
    IUserRequestRepository userRequestRepository) : ControllerBase
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

        var requests = await userRequestRepository.FindUserRequestByIpAsync(ip);
        foreach (var request in requests.Where(request => request.TokenInfo?.Token != null))
            await tokenRepository.BanTokenAsync(request.TokenInfo!.Token, $"Banned along with IP: [{reason}]");

        return Ok(bannedIp);
    }

    [HttpGet("block/token/{token}")]
    public async Task<IActionResult> BanToken(string token, [FromQuery] string reason)
    {
        var bannedToken = await tokenRepository.BanTokenAsync(token, reason);

        var requests = await userRequestRepository.FindUserRequestByTokenAsync(token);
        foreach (var request in requests)
            await ipRepository.BanIpIfNeededAsync(request.IpInfo.Ip, $"Banned along with token: [{reason}]");

        return Ok(bannedToken);
    }

    [HttpGet("unban/ip/{ip}")]
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

        var requests = await userRequestRepository.FindUserRequestByIpAsync(ip);

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

    [HttpGet("unban/token/{token}")]
    public async Task<IActionResult> UnbanToken(string token, [FromQuery] string reason)
    {
        await tokenRepository.UnbanTokenAsync(token, reason);

        var requests = await userRequestRepository.FindUserRequestByTokenAsync(token);
        foreach (var request in requests) 
            await ipRepository.UnbanIpAsync(request.IpInfo.Ip, reason);
        
        var okResponse = new
        {
            message = $"Token {token} unbanned. Also unbanned IPs, connected with this token.",
            token
        };

        return Ok(okResponse);
    }
    
    [HttpGet("whitelist/ip/{ip}")]
    public async Task<IActionResult> WhiteListIp(string ip, [FromQuery] string reason)
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

        await ipRepository.WhitelistIpAsync(ip, reason);
        var ipInfo = await ipRepository.FindIpAsync(ip);
        
        var requests = await userRequestRepository.FindUserRequestByIpAsync(ip);
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