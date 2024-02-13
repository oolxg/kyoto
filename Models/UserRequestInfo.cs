namespace Smug.Models;

public class UserRequestInfo
{
    public DateTime RequestDate { get; set; }
    public string UserIp { get; set; }
    public string? Token { get; set; }
    public string Host { get; set; }
    public string Path { get; set; }
    public Dictionary<string, string>? Headers { get; set; }

    public UserRequest AsUserRequest(Guid ipInfoId, Guid? tokenInfoId)
    {
        return new UserRequest
        {
            Id = Guid.NewGuid(),
            RequestDate = RequestDate,
            IpInfoId = ipInfoId,
            TokenInfoId = tokenInfoId,
            Host = Host,
            Path = Path,
            Headers = Headers ?? new Dictionary<string, string>(),
        };
    }
}