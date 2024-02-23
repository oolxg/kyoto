using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Smug.Models;

[Table("UserRequests")]
public class UserRequest
{
    [Key]
    [Column(TypeName = "uuid"), Required]
    public Guid Id { get; set; }
    
    [Column(TypeName = "timestamp with time zone"), Required]
    public DateTime RequestDate { get; set; }
    
    [Column(TypeName = "text"), Required]
    public string Host { get; set; }
    
    [Column(TypeName = "text"), Required]
    public string Path { get; set; }
    
    [Column(TypeName = "boolean"), Required]
    public bool IsBlocked { get; set; }
    
    [Column(TypeName = "text")]
    public string? DecisionReason { get; set; }
        
    [ForeignKey("IpId")]
    public Guid IpInfoId { get; set; }
    
    [ForeignKey("TokenId")]
    public Guid? TokenInfoId { get; set; }
    
    [JsonIgnore]
    public IpAddressInfo IpInfo { get; set; }
    [JsonIgnore]
    public TokenInfo? TokenInfo { get; set; }

    public string? Referer => Headers.TryGetValue("Referer", out var referer) ? referer : null;
    public string? UserAgent => Headers.TryGetValue("User-Agent", out var userAgent) ? userAgent : null;
    
    [Column(TypeName = "jsonb"), Required]
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
        Headers = headers ?? new Dictionary<string, string>();
    }
    
    public UserRequest()
    {
    }
}