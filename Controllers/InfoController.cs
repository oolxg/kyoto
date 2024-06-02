using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/info/")]
public class InfoController(IIpRepository ipRepository,
    ITokenRepository tokenRepository,
    IUserRequestRepository userRequestRepository) : ControllerBase
{
    [HttpGet("ip/{ip}")]
    public async Task<IActionResult> GetIpInfo(string ip)
    {
        var ipInfo = await ipRepository.FindIpAsync(ip);
        if (ipInfo == null) return NotFound();

        var tokens = await ipRepository.FindTokensByIpAsync(ip);

        var response = new
        {
            ipInfo,
            relatedTokens = tokens
        };
        
        return Ok(response);
    }

    [HttpGet("token/{token}")]
    public async Task<IActionResult> GetTokenInfo(string token)
    {
        var tokenInfo = await tokenRepository.FindTokenAsync(token);
        if (tokenInfo == null) return NotFound();

        var ips = await tokenRepository.FindIpsByTokenAsync(token);

        var response = new
        {
            tokenInfo,
            relatedIps = ips
        };

        return Ok(response);
    }
    
    [HttpGet("ip/{id:Guid}")]
    public async Task<IActionResult> GetIpInfo(Guid id)
    {
        var ipInfo = await ipRepository.FindIpAsync(id);
        if (ipInfo == null) return NotFound();

        var tokens = await ipRepository.FindTokensByIpAsync(ipInfo.Ip);

        var response = new
        {
            ipInfo,
            relatedTokens = tokens
        };
        
        return Ok(response);
    }
}