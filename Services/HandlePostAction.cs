using CatsOfMastodonBot.Models;
using JsonFlatFileDataStore;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace mstdnCats.Services
{
    public class HandlePostAction
    {
        public static async Task HandleCallbackQuery(CallbackQuery callbackQuery, IMongoCollection<Post> _db, TelegramBotClient _bot, ILogger<MastodonBot>? logger)
        {
            var config = ConfigData.fetchData();

            // Extract media ID from callback query data
            string[] parts = callbackQuery.Data.Split('-');
            if (parts.Length != 2)
            {
                logger?.LogError("Invalid callback query data format.");
                return;
            }

            string action = parts[0];
            string mediaId = parts[1];

            var filter = Builders<Post>.Filter.Eq("MediaAttachments.MediaId", mediaId);
            var post = await _db.Find(filter).FirstOrDefaultAsync();

            if (post == null)
            {
                logger?.LogInformation("No matching post found.");
                return;
            }

            // Approve the media attachment
            if (action == "approve")
            {
                var mediaAttachment = post.MediaAttachments.FirstOrDefault(m => m.MediaId == mediaId);
                if (mediaAttachment != null)
                {
                    // Check if the media attachment is already approved
                    if (mediaAttachment.Approved)
                    {
                        await _bot.AnswerCallbackQuery(callbackQuery.Id, "Media attachment is already approved.", true);
                        return;
                    }

                    // Update the media attachment
                    var update = Builders<Post>.Update.Set("MediaAttachments.$.Approved", true);
                    var result = await _db.UpdateOneAsync(filter, update);

                    if (result.ModifiedCount > 0)
                    {
                        try
                        {
                            // Send the media attachment to the channel
                            var allMediaAttachments = post.MediaAttachments.ToList();
                            await _bot.SendPhoto(config.CHANNEL_NUMID,
                                allMediaAttachments.First(m => m.MediaId == mediaId).Url,
                                caption: $"Post from " + $"<a href=\"" + post.Account.Url + "\">" +
                                         post.Account.DisplayName + " </a>", parseMode: ParseMode.Html
                                , replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("View on Mastodon", post.Url)));

                            await _bot.AnswerCallbackQuery(callbackQuery.Id,
                                "Media attachment approved and sent to channel.");
                            await _bot.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);

                            logger?.LogTrace($"Media attachment {mediaId} approved.");
                        }
                        catch (Exception e)
                        {
                            logger?.LogError($"Error while sending image to the channel:{e}");
                        }
                    }
                    else
                    {
                        logger?.LogError($"Failed to update the media attachment {mediaId}. Record might not exist or was not found.");
                    }
                }
                else
                {
                    logger?.LogError($"No media attachment found with MediaId {mediaId}.");
                }
            }

            // Reject the media attachment
            else if (action == "reject")
            {
                // Check if the post has only one attachment, if so, do not delete it, else delete the associated attachment
                if (post.MediaAttachments.Count == 1 && post.MediaAttachments.First().MediaId == mediaId)
                {
                    await _bot.AnswerCallbackQuery(callbackQuery.Id, "Post has only one attachment. No deletion performed.");
                    await _bot.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);

                    logger?.LogTrace($"Post {post.mstdnPostId} has only one attachment. No deletion performed.");
                }
                else
                {
                    var update = Builders<Post>.Update.PullFilter("MediaAttachments", Builders<MediaAttachment>.Filter.Eq("MediaId", mediaId));
                    var result = await _db.UpdateOneAsync(filter, update);

                    if (result.ModifiedCount > 0)
                    {
                        await _bot.AnswerCallbackQuery(callbackQuery.Id, "Media attachment rejected.");
                        await _bot.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);

                        logger?.LogTrace($"Media attachment {mediaId} removed from post {post.mstdnPostId}.");
                    }
                    else
                    {
                        logger?.LogError($"Failed to remove media attachment {mediaId} from post {post.mstdnPostId}.");
                    }
                }
            }
            else
            {
                logger?.LogError("Invalid action specified.");
            }
        }
    }
}