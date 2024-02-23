using Newtonsoft.Json;

namespace Smug.Models;

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

    public Dictionary<string, string>? Headers { get; set; }
    public string? Token => Headers?.TryGetValue("Token", out var token) ?? false ? token : null;

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