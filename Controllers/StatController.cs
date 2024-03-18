using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/stat/")]
public class StatController(IUserRequestRepository userRequestRepository) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayStats([FromQuery] string host = "*", [FromQuery] string path = "*")
    {
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var moscowNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);
        
        var startOfDay = new DateTime(
            moscowNow.Year, moscowNow.Month, moscowNow.Day, 0, 0, 0, DateTimeKind.Utc);
        
        var requests = await userRequestRepository
            .GetBlockedRequestsAsync(host, path, startOfDay);
        
        return Ok(requests);
    }
}