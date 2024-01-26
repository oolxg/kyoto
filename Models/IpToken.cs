using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smug.Models;

[Table("IpToken")]
public class IpToken
{
    [Key]
    [Column(TypeName = "uuid"), Required]
    public Guid Id { get; set; }
    
    [Column(TypeName = "uuid"), Required]
    public Guid IpId { get; set; }
    
    [Column(TypeName = "uuid"), Required]
    public Guid TokenId { get; set; }
    
    public IpToken(Guid ipId, Guid tokenId)
    {
        Id = Guid.NewGuid();
        IpId = ipId;
        TokenId = tokenId;
    }
    
    public IpToken()
    {
    }
}