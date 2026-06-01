using System.ComponentModel.DataAnnotations;

namespace CatsOfMastodonBot.Models;

public class MediaAttachment
{
    [Key]
    [MaxLength(50)]
    public required string Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    public required string Type { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Url { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string PreviewUrl { get; set; }
    
    [MaxLength(500)]
    public required string RemoteUrl { get; set; }
    
    [Required]
    public bool Approved { get; set; } = false;
    
    [Required]
    public bool Rejected { get; set; } = false;
    
    [MaxLength(50)]
    public string? PostId { get; set; }
    
    public Post? Post { get; set; }
}