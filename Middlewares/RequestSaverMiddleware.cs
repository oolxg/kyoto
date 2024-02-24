using System.Globalization;
using System.Text;
using Kyoto.Models;
using Kyoto.Services.Interfaces;
using Newtonsoft.Json;

namespace Kyoto.Middlewares;

public class RequestSaverMiddleware(RequestDelegate next)
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = { new DateTimeConverter() },
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
        },
        TypeNameHandling = TypeNameHandling.All
    };

    public async Task InvokeAsync(
        HttpContext context,
        IIpRepository ipRepository,
        ITokenRepository tokenRepository,
        IUserRequestRepository userRequestRepository)
    {
        try
        {
            var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
            var requestDetails = JsonConvert.DeserializeObject<UserRequestInfo>(body, _jsonSerializerSettings);

            if (requestDetails == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }
            
            if (!requestDetails.Host.EndsWith('/'))
                requestDetails.Host += '/';
            
            if (!requestDetails.Path.StartsWith('/'))
                requestDetails.Path = '/' + requestDetails.Path;
            
            // remove http or https from the host
            requestDetails.Host = requestDetails.Host
                .Replace("http://", "")
                .Replace("https://", "");

            var ipInfo = await ipRepository.FindOrCreateIpAsync(requestDetails.UserIp);
            var tokenInfo = requestDetails.Token != null
                ? await tokenRepository.FindOrCreateTokenAsync(requestDetails.Token)
                : null;
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

            await next(context);
        }
        catch (JsonSerializationException ex)
        {
            context.Response.StatusCode = 400;
            var response = new
            {
                error = true,
                description = "Invalid request body",
                exception = ex.Message
            };
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}

internal class DateTimeConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss";

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is string dateString)
            return DateTime.TryParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsedDateTime)
                ? TimeZoneInfo.ConvertTimeToUtc(parsedDateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow"))
                : Convert.ToDateTime(reader.Value);

        return Convert.ToDateTime(reader.Value);
    }

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateTimeFormat));
    }
}