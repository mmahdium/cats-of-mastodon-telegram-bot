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
        DotNetEnv.Env.Load();

        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<MastodonBot>();

        if (!CheckEnv.IsValid())
        {
            logger.LogCritical("Error reading envinonment variables, either some values are missing or no .env file was found");
            throw new Exception("Error reading envinonment variables, either some values are missing or no .env file was found");
        }
        // Setup DB
        var db = await DbInitializer.SetupDb(DotNetEnv.Env.GetString("DB_NAME"));
        if (db == null)
        {
            logger.LogCritical("Unable to setup DB");
            throw new Exception("Unable to setup DB");
        }
        logger.LogInformation("DB setup done");

        // Setup bot

        var bot = new TelegramBotClient(DotNetEnv.Env.GetString("BOT_TOKEN"));

        var me = await bot.GetMeAsync();
        await bot.DropPendingUpdatesAsync();
        bot.OnUpdate += OnUpdate;


        logger.LogInformation($"Bot is running as {me.FirstName}.");

        // Handle bot updates
        async Task OnUpdate(Update update)
        {
            if(update.Type == UpdateType.CallbackQuery){
                if(update.CallbackQuery.Data == "new_random"){ await HandleStartMessage.HandleStartMessageAsync(update.CallbackQuery.Message, bot, db, logger);}

                else {await HandlePostAction.HandleCallbackQuery(update.CallbackQuery, db, bot, logger); }
            }
            else if (update.Type == UpdateType.Message)
            {
                await OnMessage(update.Message, update.Type);
            }
            else
            {
                logger.LogInformation($"Received unhandled update {update.Type}");
            }
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
        _timer = new Timer(async _ => await RunCheck.runAsync(db, bot, DotNetEnv.Env.GetString("TAG"), logger, DotNetEnv.Env.GetString("CUSTOM_INSTANCE")), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        Console.ReadLine();
    }

}
