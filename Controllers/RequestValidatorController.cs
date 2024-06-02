using Kyoto.Models;
using Kyoto.Resources;
using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

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
        var userRequest = (UserRequest)HttpContext.Items["UserRequest"]!;
        var validationResult = await accessValidator.ValidateAsync(userRequest);

        userRequest.IsBlocked = validationResult.Block;
        userRequest.DecisionReason = validationResult.Reason;

        await userRequestRepository.UpdateUserRequestAsync(userRequest);

        if (!validationResult.Block) return Ok(validationResult);

        var tokenInfo = (TokenInfo?)HttpContext.Items["TokenInfo"];
        var ipInfo = (IpAddressInfo)HttpContext.Items["IpInfo"]!;

        await ipRepository.BanIpIfNeededAsync(ipInfo.Ip, validationResult.Reason);
        
        if (validationResult.Reason == AccessValidatorReasons.UserAgentIsEmpty ||
            validationResult.Reason == AccessValidatorReasons.BadBotUserAgent)
        {
            await ipRepository.ChangeShouldHideIfBannedAsync(ipInfo.Ip, true);
        }
        
        if (tokenInfo != null)
        {
            await tokenRepository.BanTokenAsync(tokenInfo.Token, validationResult.Reason);
        }

        return Ok(validationResult);
    }
}