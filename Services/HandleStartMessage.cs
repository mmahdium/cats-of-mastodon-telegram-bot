using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CatsOfMastodonBot.Services;

public class HandleStartMessage
{
    public static async Task HandleStartMessageAsync(Message message, TelegramBotClient _bot,
        IMongoCollection<Post> _db, ILogger<MastodonBot>? logger, CallbackQuery callbackQuery = null)
    {
        logger?.LogInformation("Start message received, trigger source: " +
                               (callbackQuery != null ? "Callback" : "Start command"));

        // choose all media attachments that are approved
        
        // OLD QUERY
        // var mediaAttachmentsToSelect = await _db.AsQueryable()
        //     .Where(post => post.MediaAttachments.Any(media => media.Approved))
        //     .ToListAsync();

        var filter = Builders<Post>.Filter.ElemMatch(post => post.MediaAttachments,
            Builders<MediaAttachment>.Filter.Eq(media => media.Approved, true));
        var projection = Builders<Post>.Projection
            .Include(p => p.Url)
            .Include(p => p.Account.DisplayName)
            .Include(p => p.MediaAttachments);
                
        var selectedPost = await _db.Aggregate().Match(filter).Project<Post>(projection).Sample(1).FirstOrDefaultAsync();
        
        // send media attachment
        await _bot.SendPhoto(message.Chat.Id,
            selectedPost.MediaAttachments.FirstOrDefault(m => m.Approved).RemoteUrl,
            $"Here is your cat!🐈\n" + "<a href=\"" + selectedPost.Url + "\">" +
            $"View on Mastodon " + " </a>", ParseMode.Html
            , replyMarkup: new InlineKeyboardMarkup()
                .AddButton(InlineKeyboardButton.WithUrl("Join channel 😺", "https://t.me/catsofmastodon"))
                .AddNewRow()
                .AddButton(InlineKeyboardButton.WithCallbackData("Send me another one!", $"new_random")));

        // answer callback query from "send me another cat" button
        if (callbackQuery != null) await _bot.AnswerCallbackQuery(callbackQuery.Id, "Catch your cat! 😺");

        logger?.LogInformation("Random cat sent!");
    }
}