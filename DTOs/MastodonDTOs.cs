using System.Text.Json.Serialization;

namespace CatsOfMastodonBot.DTOs;

public class MastodonPostDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

    [JsonPropertyName("account")] public MastodonAccountDto Account { get; set; } = new();

    [JsonPropertyName("media_attachments")]
    public List<MastodonMediaDto> MediaAttachments { get; set; } = new();
}

public class MastodonAccountDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("acct")] public string Acct { get; set; } = string.Empty;

    [JsonPropertyName("display_name")] public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("avatar_static")] public string AvatarStatic { get; set; } = string.Empty;

    [JsonPropertyName("bot")] public bool Bot { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
}

public class MastodonMediaDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("preview_url")] public string PreviewUrl { get; set; } = string.Empty;

    [JsonPropertyName("remote_url")] public string RemoteUrl { get; set; } = string.Empty;
}