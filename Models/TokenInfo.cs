using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kyoto.Models;

[Table("Tokens")]
public class TokenInfo
{
    [Key]
    [Column(TypeName = "uuid")]
    [Required]
    public Guid Id { get; set; }

    [Column(TypeName = "text")] [Required] public string Token { get; set; }

    [Column(TypeName = "integer")]
    [Required]
    public TokenStatus Status { get; private set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? StatusChangeDate { get; private set; }

    [Column(TypeName = "text")] public string? StatusChangeReason { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    [Required]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public List<IpToken> IpTokens { get; set; } = new();
    public List<UserRequest> UserRequests { get; set; } = new();

    public TokenInfo(string token)
    {
        Id = Guid.NewGuid();
        Token = token;
        Status = TokenStatus.Normal;
        CreatedAt = DateTime.UtcNow;
        IpTokens = new List<IpToken>();
        UserRequests = new List<UserRequest>();
    }

    public void UpdateStatus(TokenStatus status, string reason)
    {
        Status = status;
        StatusChangeReason = reason;
        StatusChangeDate = DateTime.UtcNow;
    }

    public TokenInfo()
    {
    }
}

public enum TokenStatus
{
    Whitelisted,
    Banned,
    Normal
}