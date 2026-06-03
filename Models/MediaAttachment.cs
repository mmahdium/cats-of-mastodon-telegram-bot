namespace CatsOfMastodonBot.Models;

public class MediaAttachment
{
    public required string Id { get; set; }

    public required string Type { get; set; }

    public required string Url { get; set; }

    public required string PreviewUrl { get; set; }

    public required string RemoteUrl { get; set; }

    public bool Approved { get; set; } = false;

    public bool Rejected { get; set; } = false;

    public string? PostId { get; set; }

    public Post? Post { get; set; }
}