using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mstdnCats.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class MastodonBot
{
    private static Timer _postFetchTimer, _backupTimer;

    private static async Task Main()
    {
        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
        var logger = loggerFactory.CreateLogger<MastodonBot>();

        // Read environment variables
        var config = ConfigData.fetchData();
        if (config == null)
        {
            logger.LogCritical(
                "Error reading environment variables, either some values are missing or no .env file was found");
            throw new Exception(
                "Error reading environment variables, either some values are missing or no .env file was found");
        }

        // Setup DB
        var db = await DbInitializer.SetupDb(config.MONGODB_CONNECTION_STRING, config.DB_NAME);
        logger.LogInformation("DB setup done");

        // Web server setup
        var host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.ListenAnyIP(5005); // Listen on port 5005
                });
                ServerStartup.Serverstartup(db);
                webBuilder.UseStartup<ServerStartup>();
            })
            .Build();

        await host.RunAsync();
        
        // Setup bot
        var bot = new TelegramBotClient(config.BOT_TOKEN);

        var me = await bot.GetMe();
        await bot.DropPendingUpdates();
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;

        logger.LogInformation("Setup complete");
        logger.LogInformation($"Bot is running as {me.FirstName} - instance: {config.INSTANCE}");

        // Handle bot updates - For glass buttons functionality
        async Task OnUpdate(Update update)
        {
            switch (update)
            {
                case { CallbackQuery: { } callbackQuery }:
                {
                    // Send a new cat picture
                    if (callbackQuery.Data == "new_random")
                    {
                        await HandleStartMessage.HandleStartMessageAsync(callbackQuery.Message, bot, db, logger,
                            callbackQuery);
                        break;
                    }

                    // Approve or reject a post
                    else if (callbackQuery.Data.Contains("approve-") || callbackQuery.Data.Contains("reject-"))
                    {
                        await HandlePostAction.HandleCallbackQuery(callbackQuery, db, bot, logger);
                        break;
                    }

                    break;
                }
                default: logger.LogInformation($"Received unhandled update {update.Type}"); break;
            }

            ;
        }

        // Handle bot messages
        async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text == "/start" && message.Chat.Type == ChatType.Private)
                await HandleStartMessage.HandleStartMessageAsync(message, bot, db, logger);
            
            else if (message.Text == "/backup" && message.Chat.Type == ChatType.Private)
                await HandleDbBackup.HandleDbBackupAsync(bot, logger, config.DB_NAME, config.ADMIN_NUMID, db);
            
            // Send a message to prompt user to send /start and recieve their cat photo only if its from a telegram user and not a channel
            else if (message.Chat.Type == ChatType.Private)
                await HandleStartMessage.HandleStartMessageAsync(message, bot, db, logger);
        }

        // Set a timer to fetch and process posts every 10 minutes
        _postFetchTimer = new Timer(async _ => await RunCheck.runAsync(db, bot, config.TAG, logger, config.INSTANCE),
            null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        // Another timer to automatically backup the DB every 6 hour
        _backupTimer =
            new Timer(
                async _ => await HandleDbBackup.HandleDbBackupAsync(bot, logger, config.DB_NAME, config.ADMIN_NUMID, db), null, TimeSpan.Zero, TimeSpan.FromHours(6));
        // Keep the bot running
        await Task.Delay(-1);
    }
}