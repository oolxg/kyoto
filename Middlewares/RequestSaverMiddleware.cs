using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Middlewares;

public class RequestSaverMiddleware(RequestDelegate next)
{
    private RequestDelegate Next => next; 
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = {new DateTimeConverter()},
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task InvokeAsync(
        HttpContext context,
        IIpRepository ipRepository,
        ITokenRepository tokenRepository,
        IUserRequestRepository userRequestRepository)
    {
        var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
        var requestDetails = JsonSerializer.Deserialize<UserRequestInfo>(body, _jsonSerializerOptions);

        if (requestDetails == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid request body");
            return;
        }

        var ipInfo = await ipRepository.FindOrCreateIpAsync(requestDetails.UserIp);
        var tokenInfo = requestDetails.Token != null ? await tokenRepository.FindOrCreateTokenAsync(requestDetails.Token) : null;
        var userRequest = requestDetails.AsUserRequest(ipInfo.Id, tokenInfo?.Id);

        await userRequestRepository.SaveUserRequestAsync(userRequest);
        
        await ipRepository.AddUserRequestToIpAsync(ipInfo.Ip, userRequest.Id);
        
        if (tokenInfo != null)
        {
            await tokenRepository.AddUserRequestToTokenAsync(tokenInfo.Token, userRequest.Id);
            await ipRepository.AddTokenAsyncIfNeeded(ipInfo.Ip, tokenInfo.Id);
        }

        context.Items["UserRequest"] = userRequest;
        context.Items["IpInfo"] = ipInfo;
        context.Items["TokenInfo"] = tokenInfo;
        
        await Next(context);
    }
}

internal class DateTimeConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.GetString() is { } dateString)
        {
            return DateTime.TryParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime)
                ? TimeZoneInfo.ConvertTimeToUtc(parsedDateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow"))
                : reader.GetDateTime();
        }

        return reader.GetDateTime();
    }


    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateTimeFormat));
    }
}
