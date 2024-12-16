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
        var mediaAttachmentsToSelect = await _db.AsQueryable()
            .Where(post => post.MediaAttachments.Any(media => media.Approved))
            .ToListAsync();

        // select random approved media attachment
        var selectedMediaAttachment = mediaAttachmentsToSelect[new Random().Next(mediaAttachmentsToSelect.Count)];
        // send media attachment
        await _bot.SendPhoto(message.Chat.Id,
            selectedMediaAttachment.MediaAttachments.FirstOrDefault(m => m.Approved == true).Url,
            $"Here is your cat!🐈\n" + "<a href=\"" + selectedMediaAttachment.Url + "\">" +
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