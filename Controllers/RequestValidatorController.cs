using Microsoft.AspNetCore.Mvc;
using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Controllers;

[ApiController]
[Route("api/v1/")]
public class RequestValidatorController(
    ITokenRepository tokenRepository, 
    IIpRepository ipRepository, 
    IUserRequestRepository userRequestRepository,
    IAccessValidator accessValidator) : ControllerBase
{
    
    [HttpPost("check")]
    public async Task<IActionResult> CheckRequest()
    {
        var userRequest = (UserRequest) HttpContext.Items["UserRequest"]!;
        var validationResultTask = accessValidator.ValidateAsync(userRequest);
        
        var validationResult = await validationResultTask;
        
        userRequest.IsBlocked = validationResult.Block;
        userRequest.BlockReason = validationResult.Reason;

        await userRequestRepository.UpdateUserRequestAsync(userRequest);
        
        if (!validationResult.Block)
        {
            return Ok(validationResult);
        }
        
        var tokenInfo = (TokenInfo?) HttpContext.Items["TokenInfo"];
        var ipInfo = (IpAddressInfo) HttpContext.Items["IpInfo"]!;

        await ipRepository.BanIpIfNeededAsync(ipInfo.Ip, validationResult.Reason);

        if (tokenInfo != null)
        {
            await tokenRepository.BanTokenAsync(tokenInfo.Token, validationResult.Reason);
        }

        if (!ipInfo.ShouldHideIfBanned)
        {
            
        }
        
        
        return Ok(validationResult);
    }
}