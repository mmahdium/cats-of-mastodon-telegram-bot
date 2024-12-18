﻿using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Services;
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
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<MastodonBot>();

        var config = configData.fetchData();
        if (config==null)
        {
            logger.LogCritical("Error reading environment variables, either some values are missing or no .env file was found");
            throw new Exception("Error reading environment variables, either some values are missing or no .env file was found");
        }
        
        // Setup DB
        Console.WriteLine("DB name: " + config.DB_NAME);
        var db = await DbInitializer.SetupDb(config.DB_NAME);
        if (db == null)
        {
            logger.LogCritical("Unable to setup DB");
            throw new Exception("Unable to setup DB");
        }
        logger.LogInformation("DB setup done");

        // Setup bot

        var bot = new TelegramBotClient(config.BOT_TOKEN);

        var me = await bot.GetMe();
        await bot.DropPendingUpdates();
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;

        logger.LogInformation("Setup complete");
        logger.LogInformation($"Bot is running as {me.FirstName}.");

        // Handle bot updates
        async Task OnUpdate(Update update)
        {
            switch (update)
            {
                case { CallbackQuery: { } callbackQuery }: {
                    if(callbackQuery.Data == "new_random"){ await HandleStartMessage.HandleStartMessageAsync(callbackQuery.Message, bot, db, logger,callbackQuery); break;}

                    else {await HandlePostAction.HandleCallbackQuery(callbackQuery, db, bot, logger); break;}
                   
                }
                default: logger.LogInformation($"Received unhandled update {update.Type}"); break;
            };
        }

        // Handle bot messages
        async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text == "/start")
            {
                await HandleStartMessage.HandleStartMessageAsync(message,bot, db, logger);
            }
            else if (message.Text == "/backup")
            {
                await HandleDbBackup.HandleDbBackupAsync(bot, logger, config.DB_NAME, config.ADMIN_NUMID, db);
            }
            // Send a message to prompt user to send /start and recieve their cat photo only if its from a telegram user and not a channel
            else if (message.Chat.Type == ChatType.Private)
            {
                await HandleStartMessage.HandleStartMessageAsync(message, bot, db, logger);
            }
        }

        // Set a timer to fetch and process posts every 15 minutes
        _postFetchTimer = new Timer(async _ => await RunCheck.runAsync(db, bot, config.TAG, logger, config.INSTANCE), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        // Another timer to automatically backup the DB every 1 hour
        _backupTimer = new Timer(async _ => await HandleDbBackup.HandleDbBackupAsync(bot, logger, config.DB_NAME, config.ADMIN_NUMID, db), null, TimeSpan.Zero, TimeSpan.FromHours(1));
        // Keep the bot running
        await Task.Delay(-1);
    }

}