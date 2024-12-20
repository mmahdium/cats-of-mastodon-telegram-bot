using System.Text;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mstdnCats.Services;

public class HandleDbBackup
{
    public static async Task HandleDbBackupAsync(TelegramBotClient _bot, ILogger<MastodonBot>? logger, string dbname,
        string adminId, IMongoCollection<Post> _db)
    {
        logger?.LogInformation("Backup requested");

        try{
        var json = (await _db.Find(new BsonDocument()).ToListAsync()).ToJson();

        var bytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);

        await _bot.SendDocument(adminId, InputFile.FromStream(stream, "backup.json"),
            "Backup of your collection\nCreated at " +
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + "\nCurrent post count: " + _db.CountDocumentsAsync(new BsonDocument())),
            ParseMode.Html);
        logger?.LogInformation("Backup sent");
        }
        catch(Exception ex){
            logger?.LogError(ex,"Unable to backup database");
        }
    }
}