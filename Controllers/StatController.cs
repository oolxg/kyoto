using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/stat/")]
public class StatController(IUserRequestRepository userRequestRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStats(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateUntil = null,
        [FromQuery] string host = "*",
        [FromQuery] string path = "*",
        [FromQuery] bool includeNotBlocked = false)
    {
        dateUntil ??= DateTime.UtcNow;
        
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var moscowNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, moscowTimeZone);

        dateFrom ??= new DateTime(moscowNow.Year, moscowNow.Month, moscowNow.Day, 0, 0, 0, DateTimeKind.Utc);

        return Ok(
            await userRequestRepository
                .GetRequestsAsync(host, path, includeNotBlocked, dateFrom.Value, dateUntil.Value)
        );
    }
}