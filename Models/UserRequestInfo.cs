namespace Smug.Models;

public class UserRequestInfo
{
    public DateTime RequestDate { get; set; }
    public string UserIp { get; set; }
    public string Host { get; set; }
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