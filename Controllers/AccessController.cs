using System.Net;
using Microsoft.AspNetCore.Mvc;
using Smug.Services.Interfaces;

namespace Smug.Controllers;

[ApiController]
[Route("api/v1/")]
public class AccessController : ControllerBase
{
    private readonly IIpRepository _ipRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUserRequestRepository _userRequestRepository;
    
    public AccessController(
        IIpRepository ipRepository,
        ITokenRepository tokenRepository,
        IUserRequestRepository userRequestRepository)
    {
        _ipRepository = ipRepository;
        _tokenRepository = tokenRepository;
        _userRequestRepository = userRequestRepository;
    }
    
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
        
        var bannedIp = await _ipRepository.BanIpAsync(ip, reason);
        
        var requests = await _userRequestRepository.FindUserRequestByIpAsync(ip);
        foreach (var request in requests.Where(request => request.Token != null))
        {
            await _tokenRepository.BanTokenAsync(request.Token!, reason);
        }
        
        return Ok(bannedIp);
    }
    
    [HttpGet("block/token/{token}")]
    public async Task<IActionResult> BanToken(string token, [FromQuery] string reason)
    {
        var bannedToken = await _tokenRepository.BanTokenAsync(token, reason);
        
        var requests = await _userRequestRepository.FindUserRequestByTokenAsync(token);
        foreach (var request in requests)
        {
            await _ipRepository.BanIpAsync(request.IpAddress, reason);
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
        
        await _ipRepository.UnbanIpAsync(ip, reason);
        
        var requests = await _userRequestRepository.FindUserRequestByIpAsync(ip);
        
        foreach (var request in requests.Where(request => request.Token != null))
        {
            await _tokenRepository.UnbanTokenAsync(request.Token!, reason);
        }
        
        return Ok();
    }
    
    [HttpGet("unban/token/{token}")]
    public async Task<IActionResult> UnbanToken(string token, [FromQuery] string reason)
    {
        await _tokenRepository.UnbanTokenAsync(token, reason);
        return Ok();
    }
}