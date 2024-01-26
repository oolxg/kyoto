using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

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
    public string IpAddress { get; set; }
    
    [Column(TypeName = "text")]
    public string? Token { get; set; }
    
    [Column(TypeName = "text")]
    public string? UserAgent { get; set; }
    
    [Column(TypeName = "text"), Required]
    public string Host { get; set; }
    
    [Column(TypeName = "text"), Required]
    public string Path { get; set; }
    
    [Column(TypeName = "jsonb"), Required]
    public Dictionary<string, string> Headers { get; set; }
    
    public UserRequest(
        string ipAddress, 
        string? token,
        string? userAgent,
        string host, 
        string path, 
        Dictionary<string, string>? headers)
    {
        Id = Guid.NewGuid();
        RequestDate = DateTime.UtcNow;
        IpAddress = ipAddress;
        Token = token;
        UserAgent = userAgent;
        Host = host;
        Path = path;
        Headers = headers ?? new Dictionary<string, string>();
    }
    
    public UserRequest(
        Guid id,
        DateTime requestDate,
        string ipAddress, 
        string? token, 
        string? userAgent,
        string host,
        string path, 
        Dictionary<string, string>? headers)
    {
        Id = id;
        RequestDate = requestDate;
        IpAddress = ipAddress;
        Token = token;
        UserAgent = userAgent;
        Host = host;
        Path = path;
        Headers = headers ?? new Dictionary<string, string>();
    }
    
    public UserRequest()
    {
    }
}