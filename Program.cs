using Microsoft.Extensions.Logging;
using mstdnCats.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

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

        // Setup DB
        var db = await DbInitializer.SetupDb(Environment.GetEnvironmentVariable("DB_NAME"));
        if (db == null)
        {
            logger.LogCritical("Unable to setup DB");
            throw new Exception("Unable to setup DB");
        }
        logger.LogInformation("DB setup done");

        // Setup bot
        var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("BOT_TOKEN"));
        var me = await bot.GetMeAsync();
        await bot.DropPendingUpdatesAsync();
        logger.LogInformation($"Bot is running as {me.FirstName}.");

        bot.OnUpdate += OnUpdate;

        // Handle bot updates
        async Task OnUpdate(Update update)
        {
            switch (update)
            {
                case { CallbackQuery: { } callbackQuery }: await HandlePostAction.HandleCallbackQuery(callbackQuery,db,bot, logger); break;
                default: logger.LogInformation($"Received unhandled update {update.Type}"); break;
            };
        }

        // Set a timer to fetch and process posts every 15 minutes
        _timer = new Timer(async _ => await RunCheck.runAsync(db, bot, Environment.GetEnvironmentVariable("TAG"),logger,Environment.GetEnvironmentVariable("CUSTOM_INSTANCE")), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        Console.ReadLine();

    }

}
