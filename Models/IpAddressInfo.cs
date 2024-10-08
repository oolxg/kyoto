using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kyoto.Models;

[Table("IpAddresses")]
public class IpAddressInfo
{
    [Key]
    [Column(TypeName = "uuid")]
    [Required]
    public Guid Id { get; set; }

    [Column(TypeName = "text")] [Required]
    public string Ip { get; set; }

    [Column(TypeName = "integer")]
    [Required]
    public IpStatus Status { get; set; }

    [Column(TypeName = "text")] 
    public string? StatusChangeReason { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? StatusChangeDate { get; set; }

    [Column(TypeName = "boolean")]
    [Required]
    public bool ShouldHideIfBanned { get; set; }

    [JsonIgnore]
    public List<IpToken> IpTokens { get; set; } = new();
    public List<UserRequest> UserRequests { get; set; } = new();

    [Column(TypeName = "timestamp with time zone")]
    [Required]
    public DateTime CreatedAt { get; set; }

    public IpAddressInfo(string ip)
    {
        Id = Guid.NewGuid();
        Ip = ip;
        Status = IpStatus.Normal;
        ShouldHideIfBanned = false;
        IpTokens = new List<IpToken>();
        UserRequests = new List<UserRequest>();
        CreatedAt = DateTime.UtcNow;
    }

    public IpAddressInfo()
    {
    }

    public void UpdateStatus(IpStatus status, string reason)
    {
        Status = status;
        StatusChangeReason = reason;
        StatusChangeDate = DateTime.UtcNow;
    }
}

public enum IpStatus
{
    Whitelisted, // 0
    Banned, // 1
    Normal // 2
}