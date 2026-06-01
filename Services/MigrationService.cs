namespace CatsOfMastodonBot.Services;

public class MigrationService
{
    private readonly Database _database;

    public MigrationService(Database database)
    {
        _database = database;
    }

    public async Task RunAsync()
    {
        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();

        var createTable = connection.CreateCommand();

        createTable.CommandText =
            """
            CREATE TABLE IF NOT EXISTS SchemaMigrations
            (
                Version TEXT PRIMARY KEY
            );
            """;

        await createTable.ExecuteNonQueryAsync();

        foreach (var file in Directory.GetFiles("Migrations", "*.sql").Order())
        {
            var version = Path.GetFileName(file);

            var check = connection.CreateCommand();

            check.CommandText =
                """
                SELECT COUNT(*)
                FROM SchemaMigrations
                WHERE Version = $version
                """;

            check.Parameters.AddWithValue("$version", version);

            var alreadyApplied =
                Convert.ToInt32(await check.ExecuteScalarAsync());

            if (alreadyApplied > 0)
                continue;

            var sql = await File.ReadAllTextAsync(file);

            var migrate = connection.CreateCommand();
            migrate.CommandText = sql;

            await migrate.ExecuteNonQueryAsync();

            var insert = connection.CreateCommand();

            insert.CommandText =
                """
                INSERT INTO SchemaMigrations(Version)
                VALUES ($version)
                """;

            insert.Parameters.AddWithValue("$version", version);

            await insert.ExecuteNonQueryAsync();
        }
    }
}