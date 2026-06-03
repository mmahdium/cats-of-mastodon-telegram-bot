using System.Net.Http.Json;
using CatsOfMastodonBot.DTOs;
using CatsOfMastodonBot.Models;
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
        client.Timeout = TimeSpan.FromSeconds(10);
        
        _config = config;
        _httpClient = client;
        _logger = logger;
    }

    public async Task<List<MastodonPostDto>> FetchCatPostsAsync(int limit = 10)
    {
        try
        {
            if (limit > 40)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be greater than 40");
            
            var response = await _httpClient.GetAsync(
                $"/api/v1/timelines/tag/catsofmastodon?limit=" + limit);
            
            response.EnsureSuccessStatusCode();
            
            var posts = await response.Content.ReadFromJsonAsync<List<MastodonPostDto>>();
            return posts ?? new List<MastodonPostDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch posts from Mastodon");
            return new List<MastodonPostDto>();
        }
    }
}