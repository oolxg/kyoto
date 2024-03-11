using Newtonsoft.Json;

namespace Kyoto.Models;

/// <summary>
/// The simplified request info that API receives from the user.
/// </summary>
public class UserRequestInfo
{
    [JsonProperty(Required = Required.Always)]
    public DateTime RequestDate { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string UserIp { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Host { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Path { get; set; }
    
    [JsonProperty(Required = Required.AllowNull)]
    public string? Token { get; set; }

    public Dictionary<string, string>? Headers { get; set; }
    /// <summary>
    /// Converts the request info to a UserRequest object(ORM entity).
    /// </summary>
    /// <param name="ipInfoId">Id of the IpInfo entity in DB, from which the request came.</param>
    /// <param name="tokenInfoId">Id of the TokenInfo entity in DB, from which the request came.</param>
    /// <returns>The UserRequest object.</returns>
    public UserRequest AsUserRequest(Guid ipInfoId, Guid? tokenInfoId)
    {
        return new UserRequest
        {
            Id = Guid.NewGuid(),
            RequestDate = RequestDate,
            Host = Host,
            Path = Path,
            IpInfoId = ipInfoId,
            TokenInfoId = tokenInfoId,
            Headers = Headers ?? new Dictionary<string, string>()
        };
    }
}