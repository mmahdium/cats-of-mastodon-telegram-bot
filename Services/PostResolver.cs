using System.Text.Json;
using Microsoft.Extensions.Logging;
using mstdnCats.Models;

namespace mstdnCats.Services
{
    public sealed class PostResolver
    {
        public static async Task<List<Post>?> GetPostsAsync(string tag, ILogger<MastodonBot>? logger, string instance)
        {
            // Get posts
            HttpClient _httpClient = new HttpClient();
            // Get posts from mastodon api (40 latest posts)
            string requestUrl = $"{instance}/api/v1/timelines/tag/{tag}?limit=40";
            var response = await _httpClient.GetAsync(requestUrl);

            // Print out ratelimit info
            logger?.LogInformation("Remaining requests: " +
                                   response.Headers.GetValues("X-RateLimit-Remaining").First() + "time to reset: " +
                                   response.Headers.GetValues("X-RateLimit-Reset").First());

            // Check if response is ok
            if (
                response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.Content.Headers.ContentType.MediaType.Contains("application/json") ||
                response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining) &&
                int.Parse(remaining.First()) != 0
            )
            {
                // Deserialize response based on 'Post' model
                return JsonSerializer.Deserialize<List<Post>>(await response.Content.ReadAsStringAsync());
            }

            else
            {
                logger?.LogCritical("Error while getting posts: " + response.StatusCode);
                return null;
            }
        }
    }
}