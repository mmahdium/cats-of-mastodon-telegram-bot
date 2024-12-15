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
        
        var json = _db.Find(new BsonDocument()).ToList().ToJson();

        var bytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);

        await _bot.SendDocument(adminId, InputFile.FromStream(stream, "backup.json"),
            "Backup of your collection\nCreated at " +
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + "\nCurrent post count: " + _db.CountDocuments(new BsonDocument())),
            ParseMode.Html);
        logger?.LogInformation("Backup sent");
    }
}