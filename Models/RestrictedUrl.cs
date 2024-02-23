using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kyoto.Models;

[Table("RestrictedUrls")]
public class RestrictedUrl
{
    [Key]
    [Column(TypeName = "uuid")]
    [Required]
    public Guid Id { get; set; }

    [Column(TypeName = "text")]
    [RegularExpression(@"^([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$")]
    [Required]
    public string Host { get; set; }

    [Column(TypeName = "text")]
    [RegularExpression(@"^\/.*\/$")]
    [Required]
    public string Path { get; set; }

    [Column(TypeName = "text")] [Required] public string Reason { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    [Required]
    public DateTime RestrictedDate { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? BannedUntil { get; set; }

    public RestrictedUrl(string host, string path, string reason, DateTime? bannedUntil = null)
    {
        Id = Guid.NewGuid();
        Host = host;
        Path = path;
        Reason = reason;
        RestrictedDate = DateTime.UtcNow;
        BannedUntil = bannedUntil;
    }

    public RestrictedUrl()
    {
    }
}