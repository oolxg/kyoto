using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
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
        var tokenInfo = (TokenInfo?) HttpContext.Items["TokenInfo"];
        var ipInfo = (IpAddressInfo) HttpContext.Items["IpInfo"]!;
        
        var validationResult = await accessValidator.ValidateAsync(userRequest);
        
        if (!validationResult.block)
        {
            return Ok(validationResult);
        }

        await ipRepository.BanIpIfNeededAsync(ipInfo.Ip, validationResult.reason);

        if (tokenInfo != null)
        {
            await tokenRepository.BanTokenAsync(tokenInfo.Token, validationResult.reason);
        }

        if (!ipInfo.ShouldHideIfBanned)
        {
            
        }
        
        
        return Ok(validationResult);
    }
}

internal static class RequestValidatorControllerConvention
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static void CheckRequestConvention()
    {
        var requestBodyModel = new OpenApiSchema
        {
            Type = "object",
            Properties =
            {
                ["RequestDate"] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "The date and time of the request"
                },
                ["UserIp"] = new OpenApiSchema
                {
                    Type = "string",
                    Description = "The IP address of the user"
                },
                ["Token"] = new OpenApiSchema
                {
                    Type = "string",
                    Description = "The token of the user",
                    Nullable = true
                },
                ["Host"] = new OpenApiSchema
                {
                    Type = "string",
                    Description = "The host of the request"
                },
                ["Path"] = new OpenApiSchema
                {
                    Type = "string",
                    Description = "The path of the request"
                },
                ["Headers"] = new OpenApiSchema
                {
                    Type = "object",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Description = "The headers of the request"
                }
            }, 
            Required = new HashSet<string>
            {
                "RequestDate",
                "UserIp",
                "Host",
                "Path"
            }
        };
        
        var requestBodyParameter = new OpenApiParameter
        {
            Name = "userRequest",
            Required = true,
            Schema = requestBodyModel
        };
        
        
    }
}