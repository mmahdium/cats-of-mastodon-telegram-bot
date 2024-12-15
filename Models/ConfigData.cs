namespace CatsOfMastodonBot.Models;

public class ConfigData
{
    public static config fetchData()
    {
        // Load from .env file first
        DotNetEnv.Env.Load();

        // Fetch values from .env file or environment variables (fall back)
        var dbName = DotNetEnv.Env.GetString("DB_NAME") ??
                     Environment.GetEnvironmentVariable("DB_NAME") ?? "catsofmastodon";
        var mongoDbConnectionString = DotNetEnv.Env.GetString("MONGODB_CONNECTION_STRING") ??
                                      Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        var botToken = DotNetEnv.Env.GetString("BOT_TOKEN") ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
        var tag = DotNetEnv.Env.GetString("TAG") ?? Environment.GetEnvironmentVariable("TAG");
        var channelNumId = DotNetEnv.Env.GetString("CHANNEL_NUMID") ??
                           Environment.GetEnvironmentVariable("CHANNEL_NUMID");
        var adminNumId = DotNetEnv.Env.GetString("ADMIN_NUMID") ?? Environment.GetEnvironmentVariable("ADMIN_NUMID");
        var instance = DotNetEnv.Env.GetString("CUSTOM_INSTANCE") ??
                       Environment.GetEnvironmentVariable("CUSTOM_INSTANCE") ?? "https://haminoa.net";

        // Check if any of the values are still null or empty
        if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(tag)
                                           || string.IsNullOrEmpty(channelNumId) || string.IsNullOrEmpty(adminNumId))
            return null; // Failure if any are missing

        // If all required variables are present, assign to the config
        var config = new config
        {
            DB_NAME = dbName,
            MONGODB_CONNECTION_STRING = mongoDbConnectionString,
            BOT_TOKEN = botToken,
            TAG = tag,
            CHANNEL_NUMID = channelNumId,
            ADMIN_NUMID = adminNumId,
            INSTANCE = instance
        };

        return config; // Success
    }

    public class config
    {
        public string DB_NAME { get; set; }
        public string BOT_TOKEN { get; set; }
        public string TAG { get; set; }
        public string CHANNEL_NUMID { get; set; }
        public string ADMIN_NUMID { get; set; }
        public string INSTANCE { get; set; }
        public string MONGODB_CONNECTION_STRING { get; set; }
    }
}