using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using mstdnCats.Models;
using Telegram.Bot;

namespace mstdnCats.Services;

public class RunCheck
{
    public static async Task<bool> runAsync(IMongoCollection<Post> _db, TelegramBotClient _bot, string _tag,
        ILogger<MastodonBot>? logger, string _instance)
    {
        // Run check
        try
        {
            // First get posts
            var posts = await PostResolver.GetPostsAsync(_tag, logger, _instance);

            if (posts == null) logger?.LogCritical("Unable to get posts");

            // Then process them
            await ProcessPosts.checkAndInsertPostsAsync(_db, _bot, posts, logger);
        }
        catch (Exception ex)
        {
            logger?.LogCritical("Error while running check: " + ex.Message);
        }

        return true;
    }
}