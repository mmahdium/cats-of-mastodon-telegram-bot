namespace CatsOfMastodonBot.Models;

public class Post
{
    public required string Id { get; set; }

    public required string Url { get; set; }

    public required Account Account { get; set; }

    public required List<MediaAttachment> MediaAttachments { get; set; }
}