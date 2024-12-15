using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using mstdnCats.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mstdnCats.Services;

public class HandleDbBackup
{
    public static async Task HandleDbBackupAsync(TelegramBotClient _bot, ILogger<MastodonBot>? logger, string dbname, string adminId,IDocumentCollection<Post> _bkDb,IMongoCollection<Post> _db)
    {
        logger?.LogInformation("Backup requested");
        
        // Retrieve all posts from DB (Exclude _id field from mongoDB since it is not needed nor implemented in Post model)
        var posts = _db.AsQueryable().ToList();
        // Retrieve all existing posts in backup DB
        var existingPosts = _bkDb.AsQueryable().ToList();
        // Insert new posts that are not in backup DB (First saves all the new ones in a list and then inserts them all at once)
        var newPosts = posts.Where(x => !existingPosts.Any(y => y.mstdnPostId == x.mstdnPostId)).ToList();
        await _bkDb.InsertManyAsync(newPosts);
        
        
        await using Stream stream = System.IO.File.OpenRead("./data/" + dbname+"_BK.json");
        var message = await _bot.SendDocument(adminId, document: InputFile.FromStream(stream, dbname+"_BK.json"),
            caption: "Backup of " + dbname + "\nCreated at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + "\nCurrent post count: " + _db.AsQueryable().Count()), parseMode: ParseMode.Html);

        logger?.LogInformation("Backup sent");


    }
}