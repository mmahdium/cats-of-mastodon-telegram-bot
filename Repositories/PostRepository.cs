using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Services;

namespace CatsOfMastodonBot.Repositories;

public class PostRepository(Database database)
{
    public async Task InsertIfNotExistsAsync(Post post)
    {
        await using var conn = database.CreateConnection();
        await conn.OpenAsync();

        await using var tx = conn.BeginTransaction();

        // Upsert account
        var accountCmd = conn.CreateCommand();

        accountCmd.Transaction = tx;

        accountCmd.CommandText =
        """
        INSERT INTO Accounts
        (
            Id,
            Username,
            Acct,
            DisplayName,
            IsBot,
            Url,
            AvatarStatic
        )
        VALUES
        (
            $id,
            $username,
            $acct,
            $displayName,
            $isBot,
            $url,
            $avatarStatic
        )
        ON CONFLICT(Id)
        DO UPDATE SET
            Username = excluded.Username,
            Acct = excluded.Acct,
            DisplayName = excluded.DisplayName,
            IsBot = excluded.IsBot,
            Url = excluded.Url,
            AvatarStatic = excluded.AvatarStatic;
        """;

        accountCmd.Parameters.AddWithValue("$id", post.Account.Id);
        accountCmd.Parameters.AddWithValue("$username", post.Account.Username);
        accountCmd.Parameters.AddWithValue("$acct", post.Account.Acct);
        accountCmd.Parameters.AddWithValue("$displayName", post.Account.DisplayName);
        accountCmd.Parameters.AddWithValue("$isBot", post.Account.IsBot);
        accountCmd.Parameters.AddWithValue("$url", post.Account.Url);
        accountCmd.Parameters.AddWithValue("$avatarStatic", post.Account.AvatarStatic);

        await accountCmd.ExecuteNonQueryAsync();

        // Insert post if missing
        var postCmd = conn.CreateCommand();

        postCmd.Transaction = tx;

        postCmd.CommandText =
        """
        INSERT OR IGNORE INTO Posts
        (
            Id,
            Url,
            AccountId
        )
        VALUES
        (
            $id,
            $url,
            $accountId
        );
        """;

        postCmd.Parameters.AddWithValue("$id", post.Id);
        postCmd.Parameters.AddWithValue("$url", post.Url);
        postCmd.Parameters.AddWithValue("$accountId", post.Account.Id);

        await postCmd.ExecuteNonQueryAsync();

        // Insert media attachments
        foreach (var media in post.MediaAttachments)
        {
            var mediaCmd = conn.CreateCommand();

            mediaCmd.Transaction = tx;

            mediaCmd.CommandText =
            """
            INSERT OR IGNORE INTO MediaAttachments
            (
                Id,
                Type,
                Url,
                PreviewUrl,
                RemoteUrl,
                Approved,
                Rejected,
                PostId
            )
            VALUES
            (
                $id,
                $type,
                $url,
                $previewUrl,
                $remoteUrl,
                0,
                0,
                $postId
            );
            """;

            mediaCmd.Parameters.AddWithValue("$id", media.Id);
            mediaCmd.Parameters.AddWithValue("$type", media.Type);
            mediaCmd.Parameters.AddWithValue("$url", media.Url);
            mediaCmd.Parameters.AddWithValue("$previewUrl", media.PreviewUrl);
            mediaCmd.Parameters.AddWithValue(
                "$remoteUrl",
                (object?)media.RemoteUrl ?? DBNull.Value);

            mediaCmd.Parameters.AddWithValue("$postId", post.Id);

            await mediaCmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
    }
}