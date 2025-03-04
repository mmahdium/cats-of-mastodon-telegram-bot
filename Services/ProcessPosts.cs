using CatsOfMastodonBot.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace mstdnCats.Services;

public class ProcessPosts
{
    public static async Task<List<MediaAttachment>> checkAndInsertPostsAsync(IMongoCollection<Post> _db,
        TelegramBotClient _bot, List<Post> fetchedPosts, ILogger<MastodonBot>? logger)
    {
        var config = ConfigData.fetchData();

        // Get existing posts
        var existingPosts = await _db.AsQueryable().Select(x => x.mstdnPostId).ToListAsync();
        logger?.LogInformation(
            $"Recieved posts to proccess: {fetchedPosts.Count} - total existing posts: {existingPosts.Count}");
        var newPosts = 0;
        // Process posts
        List<Post> validPosts = new();
        foreach (var post in fetchedPosts)
            // Check if post already exists
            if (!existingPosts.Contains(post.mstdnPostId) && post.MediaAttachments.Count > 0 &&
                post.Account.IsBot == false)
            {
                // Send approve or reject message to admin
                foreach (var media in post.MediaAttachments)
                    if (media.Type == "image")
                        try
                        {
                            await _bot.SendPhoto(config.ADMIN_NUMID, media.PreviewUrl,
                                $"<a href=\"" + post.Url + "\"> Mastodon </a>", ParseMode.Html
                                , replyMarkup: new InlineKeyboardMarkup().AddButton("Approve", $"approve-{media.MediaId}").AddButton("Reject", $"reject-{media.MediaId}"));
                            validPosts.Add(post);
                            newPosts++;
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError("Error while sending message to admin: " + ex.Message + " - Media URL: " +
                                             media.PreviewUrl);
                        }

                
            }

        // Insert post
        if (validPosts.Count != 0)
        {
            await _db.InsertManyAsync(validPosts);
        }

        logger?.LogInformation(
            $"Proccesing done, stats: received {fetchedPosts.Count} posts, inserted and sent {newPosts} new posts.");

        // Return list of media attachments
        var alldbpostsattachmentlist = _db.AsQueryable().SelectMany(x => x.MediaAttachments).ToList();
        return alldbpostsattachmentlist;
    }
}