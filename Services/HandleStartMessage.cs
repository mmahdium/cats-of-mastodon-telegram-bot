using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CatsOfMastodonBot.Services
{
    public class HandleStartMessage
    {
        public static async Task HandleStartMessageAsync(Message message, TelegramBotClient _bot, IDocumentCollection<Post> _db, ILogger<MastodonBot>? logger)
        {
            // choose all media attachments that are approved
            var mediaAttachmentsToSelect = _db.AsQueryable().Where(p => p.MediaAttachments.Any(m => m.Approved == true)).ToList();
            // select random approved media attachment
            var selectedMediaAttachment = mediaAttachmentsToSelect[new Random().Next(mediaAttachmentsToSelect.Count)];
            // send media attachment
            await _bot.SendPhotoAsync(message.Chat.Id, selectedMediaAttachment.MediaAttachments.FirstOrDefault(m => m.Approved == true).Url,
            caption: $"<a href=\"" + selectedMediaAttachment.Url + "\">" + $"Here is your cat!" + "</br>" + "View on Mastodon 🐈" + " </a>", parseMode: ParseMode.Html
                        , replyMarkup: new InlineKeyboardMarkup().AddButton(InlineKeyboardButton.WithUrl("Join channel 😺", selectedMediaAttachment.Url))
                        .AddNewRow()
                        .AddButton(InlineKeyboardButton.WithCallbackData("Send me another one!", $"new_random")));


        }
    }
}