using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kyoto.Models;

[Table("UserRequests")]
public class UserRequest
{
    [Key]
    [Column(TypeName = "uuid")]
    [Required]
    public Guid Id { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    [Required]
    public DateTime RequestDate { get; set; }

    [Column(TypeName = "text")] [Required] public string Host { get; set; }

    [Column(TypeName = "text")] [Required] public string Path { get; set; }

    [Column(TypeName = "boolean")]
    [Required]
    public bool IsBlocked { get; set; }
    
    [Column(TypeName = "boolean")]
    [Required]
    public bool IsHidden { get; set; }

    [Column(TypeName = "text")] public string? DecisionReason { get; set; }

    [ForeignKey("IpId")] public Guid IpInfoId { get; set; }

    [ForeignKey("TokenId")] public Guid? TokenInfoId { get; set; }

    public IpAddressInfo IpInfo { get; set; }
    public TokenInfo? TokenInfo { get; set; }

    public string? Referer => Headers.GetValueOrDefault("Referer");
    public string? UserAgent => Headers.GetValueOrDefault("User-Agent");

    [Column(TypeName = "jsonb")]
    [Required]
    public Dictionary<string, string> Headers { get; set; }

    public UserRequest(
        Guid ipId,
        Guid? tokenId,
        string host,
        string path,
        Dictionary<string, string>? headers = null)
    {
        Id = Guid.NewGuid();
        RequestDate = DateTime.UtcNow;
        IpInfoId = ipId;
        TokenInfoId = tokenId;
        Host = host;
        Path = path;
        IsBlocked = false;
        IsHidden = false;
        Headers = headers ?? new Dictionary<string, string>();
    }

    public UserRequest(
        Guid id,
        DateTime requestDate,
        Guid ipId,
        Guid? tokenId,
        string host,
        string path,
        Dictionary<string, string>? headers)
    {
        Id = id;
        RequestDate = requestDate;
        IpInfoId = ipId;
        TokenInfoId = tokenId;
        Host = host;
        Path = path;
        IsBlocked = false;
        IsHidden = false;
        Headers = headers ?? new Dictionary<string, string>();
    }

    public UserRequest()
    {
    }
}