using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mstdnCats.Services;

public class HandleDbBackup
{
    public static async Task HandleDbBackupAsync(TelegramBotClient _bot, ILogger<MastodonBot>? logger, string dbname, string adminId,IDocumentCollection<Post> _db)
    {
        logger?.LogInformation("Backup requested");
        
        await using Stream stream = System.IO.File.OpenRead("./" + dbname+".json");
        var message = await _bot.SendDocumentAsync(adminId, document: InputFile.FromStream(stream, dbname+".json"),
            caption: "Backup of " + dbname + "\nCreated at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + "\nCurrent post count: " + _db.AsQueryable().Count()), parseMode: ParseMode.Html);

        logger?.LogInformation("Backup sent");


    }
}