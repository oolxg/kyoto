using System.Globalization;
using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/stat/")]
public class StatController(IUserRequestRepository userRequestRepository) : ControllerBase
{
    // redirect to /api/v1/stat?dateFrom={Begin of the day}&dateUntil={Now}
    [HttpGet("today")]
    public IActionResult GetTodayStats()
    {
        return Redirect($"/api/v1/stat?start={DateTime.UtcNow.Date:yyyy-MM-dd}&end={DateTime.UtcNow:yyyy-MM-dd}");
    }
    
    [HttpGet]
    public async Task<IActionResult> GetStats(
        [FromQuery(Name = "start")] string? startString = null,
        [FromQuery(Name = "end")] string? endString = null,
        [FromQuery] string host = "*",
        [FromQuery] string path = "*",
        [FromQuery] bool includeNotBlocked = false)
    {
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var moscowNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, moscowTimeZone);

        if (!DateTime.TryParseExact(startString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            start = moscowNow.Date;
        
        start = start.ToUniversalTime();

        if (!DateTime.TryParseExact(endString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            end = DateTime.UtcNow;
        
        end = end.ToUniversalTime();

        if (start > end)
        {
            var msg = new
            {
                error = true,
                description = "Start date is greater than end date"
            };
            return BadRequest(msg);
        }

        return Ok(
            await userRequestRepository
                .GetRequestsAsync(host, path, includeNotBlocked, start, end)
        );

    }
}