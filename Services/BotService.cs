using System.Net;
using CatsOfMastodonBot.DTOs;
using CatsOfMastodonBot.Repositories;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CatsOfMastodonBot.Services;

public class BotService
{
    private readonly TelegramBotClient _botClient;
    private readonly AppConfig _config;
    private readonly ILogger<BotService> _logger;
    private readonly MediaAttachmentRepository _mediaAttachmentRepository;
    private readonly PostRepository _postRepository;

    public BotService(AppConfig config, PostRepository postRepository,
        MediaAttachmentRepository mediaAttachmentRepository, ILogger<BotService> logger)
    {
        if (config.Socks5Proxy is not null)
        {
            var webProxy = new WebProxy(config.Socks5Proxy);
            var botHttpClient = new HttpClient(new SocketsHttpHandler { Proxy = webProxy, UseProxy = true });
            _botClient = new TelegramBotClient(config.TelegramBotToken, botHttpClient);
        }
        else
        {
            _botClient = new TelegramBotClient(config.TelegramBotToken);
        }

        _config = config;
        _logger = logger;
        _postRepository = postRepository;
        _mediaAttachmentRepository = mediaAttachmentRepository;

        _botClient.DropPendingUpdates();
        _botClient.OnMessage += OnMessage;
        _botClient.OnUpdate += OnUpdate;
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        await _botClient.SendMessage(message.Chat.Id, "See you here!🐈\n@catsofmastodon");
    }

    private async Task OnUpdate(Update update)
    {
        switch (update)
        {
            case { CallbackQuery: { } callbackQuery }:
            {
                if (callbackQuery.Data == null || callbackQuery.Message == null)
                {
                    _logger.LogError($"Received null callback query data or message{callbackQuery.Data}");
                }

                // Approve or reject a post
                else if (callbackQuery.Data.Contains("approve-") || callbackQuery.Data.Contains("reject-"))
                {
                    var parts = callbackQuery.Data.Split('-');
                    if (parts.Length != 2)
                    {
                        _logger.LogError("Invalid callback query data format.");
                        return;
                    }

                    var action = parts[0];
                    var mediaId = parts[1];

                    switch (action)
                    {
                        case "approve":
                            var approveMediaResult = await _mediaAttachmentRepository.ApproveAsync(mediaId);
                            if (approveMediaResult is not null)
                            {
                                await _botClient.SendPhoto(_config.ChannelNumericId,
                                    approveMediaResult.MediaAttachment.RemoteUrl,
                                    $"Post from " + $"<a href=\"" + approveMediaResult.Post.Url + "\">" +
                                    approveMediaResult.Account.DisplayName + " </a>", ParseMode.Html);

                                await _botClient.AnswerCallbackQuery(callbackQuery.Id,
                                    "Media attachment approved and sent to channel.");
                                await _botClient.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);
                            }
                            else
                            {
                                await _botClient.AnswerCallbackQuery(callbackQuery.Id,
                                    "Media attachment was approved before.");
                                await _botClient.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);
                            }

                            break;

                        case "reject":
                            var rejectMediaResult = await _mediaAttachmentRepository.RejectAsync(mediaId);
                            if (rejectMediaResult > 0)
                            {
                                await _botClient.AnswerCallbackQuery(callbackQuery.Id,
                                    "Media attachment rejected successfully ");
                                await _botClient.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);
                            }
                            else
                            {
                                await _botClient.AnswerCallbackQuery(callbackQuery.Id,
                                    "Media attachment was rejected before.");
                                await _botClient.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id);
                            }

                            break;
                        default:
                            _logger.LogError($"Invalid action: {action}");
                            return;
                    }
                }
                /*if (callbackQuery.Data == "new_random")
                {
                    await HandleStartMessage.HandleStartMessageAsync(callbackQuery.Message, bot, db, logger,
                        callbackQuery);
                    break;
                }*/

                else
                {
                    _logger.LogError($"Received unhandled callback query {callbackQuery.Data}");
                }

                break;
            }
            default: _logger.LogInformation($"Received unhandled update {update.Type}"); break;
        }
    }


    public async Task SendPostToAdmin(MastodonPostDto post)
    {
        foreach (var media in post.MediaAttachments)
            try
            {
                await _botClient.SendPhoto(_config.AdminNumericId, media.PreviewUrl,
                    $"<a href=\"" + post.Url + "\"> Mastodon </a>", ParseMode.Html
                    , replyMarkup: new InlineKeyboardMarkup().AddButton("Approve", $"approve-{media.Id}").AddButton("Reject", $"reject-{media.Id}"));

                _logger.LogInformation("Sent message to admin: " + media.PreviewUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while sending message to admin: " + ex.Message + " - Media URL: " +
                                 media.PreviewUrl);
            }
    }
}