using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Services;

namespace CatsOfMastodonBot.Repositories;

public class PostRepository(Database database)
{
    public async Task<int> InsertIfNotExistsAsync(Post post)
    {
        await using var conn = database.CreateConnection();
        await conn.OpenAsync();

        await using var tx = conn.BeginTransaction();

        // Step 1: Check if account exists by Acct (the stable identifier)
        var findAccountCmd = conn.CreateCommand();
        findAccountCmd.Transaction = tx;
        findAccountCmd.CommandText = "SELECT Id FROM Accounts WHERE Acct = $acct;";
        findAccountCmd.Parameters.AddWithValue("$acct", post.Account.Acct);

        string accountIdToUse;

        if (await findAccountCmd.ExecuteScalarAsync() is string existingAccountId)
        {
            // Account exists - use the existing ID from database
            accountIdToUse = existingAccountId;

            // Update the account info, but preserve the existing Id
            var updateAccountCmd = conn.CreateCommand();
            updateAccountCmd.Transaction = tx;
            updateAccountCmd.CommandText =
                """
                UPDATE Accounts 
                SET Username = $username,
                    DisplayName = $displayName,
                    IsBot = $isBot,
                    Url = $url,
                    AvatarStatic = $avatarStatic
                WHERE Acct = $acct;
                """;
            updateAccountCmd.Parameters.AddWithValue("$username", post.Account.Username);
            updateAccountCmd.Parameters.AddWithValue("$acct", post.Account.Acct);
            updateAccountCmd.Parameters.AddWithValue("$displayName", post.Account.DisplayName);
            updateAccountCmd.Parameters.AddWithValue("$isBot", post.Account.IsBot);
            updateAccountCmd.Parameters.AddWithValue("$url", post.Account.Url);
            updateAccountCmd.Parameters.AddWithValue("$avatarStatic", post.Account.AvatarStatic);

            await updateAccountCmd.ExecuteNonQueryAsync();
        }
        else
        {
            // New account - insert with the provided Id
            accountIdToUse = post.Account.Id;

            var insertAccountCmd = conn.CreateCommand();
            insertAccountCmd.Transaction = tx;
            insertAccountCmd.CommandText =
                """
                INSERT INTO Accounts
                (Id, Username, Acct, DisplayName, IsBot, Url, AvatarStatic)
                VALUES
                ($id, $username, $acct, $displayName, $isBot, $url, $avatarStatic);
                """;
            insertAccountCmd.Parameters.AddWithValue("$id", post.Account.Id);
            insertAccountCmd.Parameters.AddWithValue("$username", post.Account.Username);
            insertAccountCmd.Parameters.AddWithValue("$acct", post.Account.Acct);
            insertAccountCmd.Parameters.AddWithValue("$displayName", post.Account.DisplayName);
            insertAccountCmd.Parameters.AddWithValue("$isBot", post.Account.IsBot);
            insertAccountCmd.Parameters.AddWithValue("$url", post.Account.Url);
            insertAccountCmd.Parameters.AddWithValue("$avatarStatic", post.Account.AvatarStatic);

            await insertAccountCmd.ExecuteNonQueryAsync();
        }

        // Step 2: Insert post using the correct AccountId (from database, not from the incoming data)
        var postCmd = conn.CreateCommand();
        postCmd.Transaction = tx;
        postCmd.CommandText =
            """
            INSERT OR IGNORE INTO Posts
            (Id, Url, AccountId)
            VALUES
            ($id, $url, $accountId);
            """;
        postCmd.Parameters.AddWithValue("$id", post.Id);
        postCmd.Parameters.AddWithValue("$url", post.Url);
        postCmd.Parameters.AddWithValue("$accountId", accountIdToUse); // Use the stable ID

        var postRowsAffected = await postCmd.ExecuteNonQueryAsync();

        if (postRowsAffected == 0)
        {
            await tx.CommitAsync();
            return 0;
        }

        // Step 3: Insert media attachments
        foreach (var media in post.MediaAttachments)
        {
            var mediaCmd = conn.CreateCommand();
            mediaCmd.Transaction = tx;
            mediaCmd.CommandText =
                """
                INSERT OR IGNORE INTO MediaAttachments
                (Id, Type, Url, PreviewUrl, RemoteUrl, Approved, Rejected, PostId)
                VALUES
                ($id, $type, $url, $previewUrl, $remoteUrl, 0, 0, $postId);
                """;
            mediaCmd.Parameters.AddWithValue("$id", media.Id);
            mediaCmd.Parameters.AddWithValue("$type", media.Type);
            mediaCmd.Parameters.AddWithValue("$url", media.Url);
            mediaCmd.Parameters.AddWithValue("$previewUrl", media.PreviewUrl);
            mediaCmd.Parameters.AddWithValue("$remoteUrl", (object?)media.RemoteUrl ?? DBNull.Value);
            mediaCmd.Parameters.AddWithValue("$postId", post.Id);

            await mediaCmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
        return 1;
    }
}