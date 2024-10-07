using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Services;
using Microsoft.Extensions.Logging;
using mstdnCats.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class MastodonBot
{


    private static Timer _timer;

    private static async Task Main()
    {
        

        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<MastodonBot>();

        if (!configData.fetchData())
        {
            logger.LogCritical("Error reading envinonment variables, either some values are missing or no .env file was found");
            throw new Exception("Error reading envinonment variables, either some values are missing or no .env file was found");
        }
        var config = new configData.config();
        // Setup DB
        var db = await DbInitializer.SetupDb(config.DB_NAME);
        if (db == null)
        {
            logger.LogCritical("Unable to setup DB");
            throw new Exception("Unable to setup DB");
        }
        logger.LogInformation("DB setup done");

        // Setup bot

        var bot = new TelegramBotClient(config.BOT_TOKEN);

        var me = await bot.GetMeAsync();
        await bot.DropPendingUpdatesAsync();
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;


        logger.LogInformation($"Bot is running as {me.FirstName}.");

        // Handle bot updates
        async Task OnUpdate(Update update)
        {
            switch (update)
            {
                case { CallbackQuery: { } callbackQuery }: {
                    if(callbackQuery.Data == "new_random"){ await HandleStartMessage.HandleStartMessageAsync(callbackQuery.Message, bot, db, logger); break;}

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
        }

        // Set a timer to fetch and process posts every 15 minutes
        _timer = new Timer(async _ => await RunCheck.runAsync(db, bot, config.TAG, logger, config.INSTANCE), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        Console.ReadLine();
    }

}

internal class ConfigData
{
}