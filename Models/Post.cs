using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.Bot.Types;


namespace mstdnCats.Models;

[BsonIgnoreExtraElements]
public class Post
{
    [JsonPropertyName("id")] public required string mstdnPostId { get; set; }
    [JsonPropertyName("url")] public required string Url { get; set; }
    [JsonPropertyName("account")] public required Account Account { get; set; }

    [JsonPropertyName("media_attachments")]
    public required List<MediaAttachment> MediaAttachments { get; set; }
}

public class Account
{
    [JsonPropertyName("id")] public required string AccId { get; set; }
    [JsonPropertyName("username")] public required string Username { get; set; }
    [JsonPropertyName("acct")] public required string Acct { get; set; }
    [JsonPropertyName("display_name")] public required string DisplayName { get; set; }
    [JsonPropertyName("bot")] public required bool IsBot { get; set; }
    [JsonPropertyName("url")] public required string Url { get; set; }
    [JsonPropertyName("avatar_static")] public required string AvatarStatic { get; set; }
}

public class MediaAttachment
{
    [JsonPropertyName("id")] public required string MediaId { get; set; }
    [JsonPropertyName("type")] public required string Type { get; set; }
    [JsonPropertyName("url")] public required string Url { get; set; }
    [JsonPropertyName("preview_url")] public required string PreviewUrl { get; set; }
    [JsonPropertyName("remote_url")] public required string RemoteUrl { get; set; }
    public bool Approved { get; set; } = false;
}