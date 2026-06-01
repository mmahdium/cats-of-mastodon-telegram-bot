using Microsoft.Data.Sqlite;

namespace CatsOfMastodonBot.Services;

public sealed class Database(AppConfig config)
{
    private readonly string _connectionString = $"Data Source={config.DbPath}";

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}