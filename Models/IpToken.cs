using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kyoto.Models;

[Table("IpToken")]
[PrimaryKey("IpAddressInfoId", "TokenInfoId")]
public class IpToken
{
    [Column(TypeName = "uuid"), Required] public Guid IpAddressInfoId { get; set; }

    [Column(TypeName = "uuid"), Required] public Guid TokenInfoId { get; set; }

    public IpToken(Guid ipAddressInfoId, Guid tokenInfoId)
    {
        IpAddressInfoId = ipAddressInfoId;
        TokenInfoId = tokenInfoId;
    }

    public IpToken()
    {
    }
}