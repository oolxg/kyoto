using System.Globalization;
using Kyoto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kyoto.Controllers;

[ApiController]
[Route("api/v1/stat/")]
public class StatController(IUserRequestRepository userRequestRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStats(
        [FromQuery(Name = "start")] string? startString = null,
        [FromQuery(Name = "end")] string? endString = null,
        [FromQuery] string host = "*",
        [FromQuery] string path = "*",
        [FromQuery] bool includeHidden = false,
        [FromQuery] bool includeNotBlocked = false)
    {
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var moscowNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, moscowTimeZone);

        if (!DateTime.TryParseExact(startString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            start = moscowNow.Date;
        
        start = start.ToUniversalTime();

        if (!DateTime.TryParseExact(endString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            end = moscowNow.Date.AddDays(1);
        
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

        var requests = await userRequestRepository
            .GetRequestsAsync(
                host,
                path,
                includeNotBlocked,
                includeHidden,
                start, 
                end);
        
        return Ok(requests);
    }
}