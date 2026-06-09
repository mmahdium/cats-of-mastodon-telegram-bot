using System.Text.Json;
using System.Text.Json.Serialization;
using CatsOfMastodonBot.DTOs;
using Microsoft.Extensions.Logging;

namespace CatsOfMastodonBot.Services;

public class MastodonService
{
    private readonly AppConfig _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MastodonService> _logger;

    public MastodonService(AppConfig config, ILogger<MastodonService> logger)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(config.MastodonInstance);
        client.DefaultRequestHeaders.Add("User-Agent", "CatsOfMastodonBot");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);

        _config = config;
        _httpClient = client;
        _logger = logger;
    }

    public async Task<List<MastodonPostDto>> FetchCatPostsAsync(int limit)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/timelines/tag/catsofmastodon?limit=" + limit);

            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            var posts = await JsonSerializer.DeserializeAsync(responseStream,
                AppJsonSerializerContext.Default.ListMastodonPostDto);

            return posts ?? new List<MastodonPostDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch posts from Mastodon");
            return new List<MastodonPostDto>();
        }
    }
}

/*[JsonSerializable(typeof(MastodonPostDto))]
[JsonSerializable(typeof(MastodonAccountDto))]
[JsonSerializable(typeof(MastodonMediaDto))]*/
[JsonSerializable(typeof(List<MastodonPostDto>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}