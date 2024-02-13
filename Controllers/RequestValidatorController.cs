using Microsoft.AspNetCore.Mvc;
using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Controllers;

[ApiController]
[Route("api/v1/")]
public class RequestValidatorController(
    ITokenRepository tokenRepository, 
    IIpRepository ipRepository, 
    IUserRequestRepository userRequestRepository) : ControllerBase
{
    private ITokenRepository TokenRepository => tokenRepository;
    private IIpRepository IpRepository => ipRepository;
    private IUserRequestRepository UserRequestRepository => userRequestRepository;
    
    [HttpPost("check")]
    public async Task<IActionResult> CheckRequest()
    {
        var userRequest = (UserRequest) HttpContext.Items["UserRequest"]!;
        
        return Ok(userRequest);
    }
}