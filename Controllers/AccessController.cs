using System.Net;
using Microsoft.AspNetCore.Mvc;
using Smug.Services.Interfaces;

namespace Smug.Controllers;

[ApiController]
[Route("api/v1/")]
public class AccessController(
    IIpRepository ipRepository, 
    ITokenRepository tokenRepository,
    IUserRequestRepository userRequestRepository) : ControllerBase
{
    private IIpRepository IpRepository => ipRepository;
    private ITokenRepository TokenRepository => tokenRepository;
    private IUserRequestRepository UserRequestRepository => userRequestRepository;
    
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
        
        var bannedIp = await IpRepository.BanIpAsync(ip, reason);
        
        var requests = await UserRequestRepository.FindUserRequestByIpAsync(ip);
        foreach (var request in requests.Where(request => request.TokenInfo?.Token != null))
        {
            await TokenRepository.BanTokenAsync(request.TokenInfo!.Token, reason);
        }
        
        return Ok(bannedIp);
    }
    
    [HttpGet("block/token/{token}")]
    public async Task<IActionResult> BanToken(string token, [FromQuery] string reason)
    {
        var bannedToken = await TokenRepository.BanTokenAsync(token, reason);
        
        var requests = await UserRequestRepository.FindUserRequestByTokenAsync(token);
        foreach (var request in requests)
        {
            await IpRepository.BanIpAsync(request.IpInfo.Ip, reason);
        }
        
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
        
        await IpRepository.UnbanIpAsync(ip, reason);
        
        var requests = await UserRequestRepository.FindUserRequestByIpAsync(ip);
        
        foreach (var request in requests.Where(request => request.TokenInfo?.Token != null))
        {
            await TokenRepository.UnbanTokenAsync(request.TokenInfo!.Token, reason);
        }
        
        return Ok();
    }
    
    [HttpGet("unban/token/{token}")]
    public async Task<IActionResult> UnbanToken(string token, [FromQuery] string reason)
    {
        await TokenRepository.UnbanTokenAsync(token, reason);
        
        var requests = await UserRequestRepository.FindUserRequestByTokenAsync(token);
        foreach (var request in requests)
        {
            await IpRepository.UnbanIpAsync(request.IpInfo.Ip, reason);
        }
        
        return Ok();
    }
}