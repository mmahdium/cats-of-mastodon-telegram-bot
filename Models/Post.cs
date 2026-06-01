using System.ComponentModel.DataAnnotations;

namespace CatsOfMastodonBot.Models;

public class Post
{
    [Key]
    [MaxLength(50)]
    public required string Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Url { get; set; }
    
    [Required]
    public required Account Account { get; set; }
    
    [Required]
    public required List<MediaAttachment> MediaAttachments { get; set; }
}