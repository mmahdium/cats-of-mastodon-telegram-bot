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
    }

    private async Task OnUpdate(Update update)
    {
    }


    private async Task SendPostToChannel(MastodonPostDto post)
    {
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