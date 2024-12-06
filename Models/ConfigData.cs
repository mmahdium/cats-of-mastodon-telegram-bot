namespace CatsOfMastodonBot.Models
{
    public class ConfigData
    {

        public static config fetchData()
        {
            // Load from .env file first
            DotNetEnv.Env.Load();

            // Fetch values from .env file or environment variables (fall back)
            string dbName = DotNetEnv.Env.GetString("DB_NAME") ?? Environment.GetEnvironmentVariable("DB_NAME");
            string mongoDbConnectionString = DotNetEnv.Env.GetString("MONGODB_CONNECTION_STRING") ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            string botToken = DotNetEnv.Env.GetString("BOT_TOKEN") ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
            string tag = DotNetEnv.Env.GetString("TAG") ?? Environment.GetEnvironmentVariable("TAG");
            string channelNumId = DotNetEnv.Env.GetString("CHANNEL_NUMID") ?? Environment.GetEnvironmentVariable("CHANNEL_NUMID");
            string adminNumId = DotNetEnv.Env.GetString("ADMIN_NUMID") ?? Environment.GetEnvironmentVariable("ADMIN_NUMID");
            string instance = DotNetEnv.Env.GetString("CUSTOM_INSTANCE") ?? Environment.GetEnvironmentVariable("CUSTOM_INSTANCE") ?? "https://haminoa.net";

            // Check if any of the values are still null or empty
            if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(tag)
                || string.IsNullOrEmpty(channelNumId) || string.IsNullOrEmpty(adminNumId))
            {
                return null; // Failure if any are missing
            }

            // If all required variables are present, assign to the config
            config config = new config
            {
                DB_NAME = dbName,
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
        }
    }
}