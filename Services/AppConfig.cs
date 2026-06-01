namespace CatsOfMastodonBot.Services;

public class AppConfig
{
    public required string DbPath { get; set; }
    public required string TelegramBotToken { get; set; }
    public required string AdminNumericId { get; set; }
    public required string ChannelNumericId { get; set; }
    public required string MastodonInstance { get; set; }
    public string? Socks5Proxy { get; set; }
}

public static class AppConfigLoader
{
    public static AppConfig LoadConfigFromEnv()
    {
        var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        var adminId = Environment.GetEnvironmentVariable("ADMIN_NUMERIC_ID");
        var channelId = Environment.GetEnvironmentVariable("CHANNEL_NUMERIC_ID");

        if (string.IsNullOrEmpty(telegramToken))
            throw new Exception("TELEGRAM_BOT_TOKEN environment variable is required");

        if (string.IsNullOrEmpty(adminId))
            throw new Exception("ADMIN_NUMERIC_ID environment variable is required");

        if (string.IsNullOrEmpty(channelId))
            throw new Exception("CHANNEL_NUMERIC_ID environment variable is required");

        var customInstance = Environment.GetEnvironmentVariable("MASTODON_INSTANCE");
        if (!string.IsNullOrEmpty(customInstance) && customInstance.EndsWith("/"))
            customInstance = customInstance.Substring(0, customInstance.Length - 1);

        var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
        if (string.IsNullOrEmpty(dbPath))
        {
            dbPath = "/data/com_bot.db";
        }
        else if (!(dbPath.EndsWith(".db") || dbPath.EndsWith(".sqlite") || dbPath.EndsWith(".sqlite3")))
        {
            Directory.CreateDirectory(dbPath);
            dbPath = Path.Combine(dbPath, "com_bot.db");
        }

        var socks5Proxy = Environment.GetEnvironmentVariable("SOCKS5_PROXY");
        // try parse the url to be valid like socks5://tor:9050
        if (!string.IsNullOrEmpty(socks5Proxy))
            if (Uri.TryCreate(socks5Proxy, UriKind.Absolute, out var proxyUri) && proxyUri.Scheme == "socks5")
                socks5Proxy = proxyUri.ToString();


        return new AppConfig
        {
            TelegramBotToken = telegramToken,
            AdminNumericId = adminId,
            ChannelNumericId = channelId,
            DbPath = dbPath,
            MastodonInstance = customInstance ?? "https://haminoa.net",
            Socks5Proxy = socks5Proxy
        };
    }
}