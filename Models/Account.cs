namespace CatsOfMastodonBot.Models;

public class Account
{
    public required string Id { get; set; }

    public required string Username { get; set; }

    public required string Acct { get; set; }

    public required string DisplayName { get; set; }

    public required bool IsBot { get; set; }

    public required string Url { get; set; }

    public required string AvatarStatic { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}