using CatsOfMastodonBot.Models;
using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace mstdnCats.Services
{
    public class ProcessPosts
    {
        public static async Task<List<MediaAttachment>> checkAndInsertPostsAsync(IDocumentCollection<Post> _db, TelegramBotClient _bot, List<Post> fetchedPosts, ILogger<MastodonBot>? logger)
        {
            var config = configData.fetchData();

            // Get existing posts
            var existingPosts = _db.AsQueryable().Select(x => x.mstdnPostId).ToArray();
            logger?.LogInformation($"Recieved posts to proccess: {fetchedPosts.Count} - total existing posts: {existingPosts.Length}");
            int newPosts = 0;
            // Process posts
            foreach (Post post in fetchedPosts)
            {
                // Check if post already exists
                if (!existingPosts.Contains(post.mstdnPostId) && post.MediaAttachments.Count > 0)
                {

                    // Send approve or reject message to admin
                    foreach (var media in post.MediaAttachments)
                    {
                        if (media.Type == "image")
                        {
                            try
                            {
                                await _bot.SendPhoto(config.ADMIN_NUMID, media.PreviewUrl, caption: $"<a href=\"" + post.Url + "\"> Mastodon </a>", parseMode: ParseMode.Html
                            , replyMarkup: new InlineKeyboardMarkup().AddButton("Approve", $"approve-{media.MediaId}").AddButton("Reject", $"reject-{media.MediaId}"));

                                // Insert post
                                await _db.InsertOneAsync(post);
                                newPosts++;
                            }
                            catch (System.Exception ex)
                            {
                                logger?.LogError("Error while sending message to admin: " + ex.Message + " - Media URL: " + media.PreviewUrl);
                            }
                        }
                    }

                }
            }

            logger?.LogInformation($"Proccesing done, stats: received {fetchedPosts.Count} posts, inserted {newPosts} new posts.");

            // Return list of media attachments
            var alldbpostsattachmentlist = _db.AsQueryable().SelectMany(x => x.MediaAttachments).ToList();
            return alldbpostsattachmentlist;
        }
    }
}