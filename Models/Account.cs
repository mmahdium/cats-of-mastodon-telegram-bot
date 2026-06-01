using System.ComponentModel.DataAnnotations;

namespace CatsOfMastodonBot.Models;

public class Account
{
    [Key]
    [MaxLength(50)]
    public required string Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Username { get; set; }
    
    [Required]
    [MaxLength(150)]
    public required string Acct { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string DisplayName { get; set; }
    
    [Required]
    public required bool IsBot { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Url { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string AvatarStatic { get; set; }
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}