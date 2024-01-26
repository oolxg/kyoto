using System.Net;
using Microsoft.AspNetCore.Mvc;
using Smug.Services.Interfaces;
using Smug.Utils;

namespace Smug.Controllers;

[ApiController]
[Route("api/v1/block")]
public class AccessController : ControllerBase
{
    private readonly IIpRepository _ipRepository;
    
    public AccessController(IIpRepository ipRepository)
    {
        _ipRepository = ipRepository;
    }
    
    [HttpGet("ip/{ip}")]
    public async Task<IActionResult> BanIp(string ip, [FromQuery] string? reason)
    {
        if (IPAddress.TryParse(ip, out _) == false)
        {
            var response = new
            {
                error = true,
                description = "Invalid IP address"
            };
            
            return BadRequest(response);
        }
        
        var bannedIp = await _ipRepository.BanIpAsync(ip, reason);
        return Ok(bannedIp);
    }
}