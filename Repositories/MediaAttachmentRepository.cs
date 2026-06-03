using CatsOfMastodonBot.DTOs;
using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Services;

namespace CatsOfMastodonBot.Repositories;

public class MediaAttachmentRepository(Database database)
{
    public async Task ApproveAsync(string mediaId)
    {
        await using var conn = database.CreateConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();

        cmd.CommandText =
            """
            UPDATE MediaAttachments
            SET Approved = 1,
                Rejected = 0
            WHERE Id = $id;
            """;

        cmd.Parameters.AddWithValue("$id", mediaId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RejectAsync(string mediaId)
    {
        await using var conn = database.CreateConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();

        cmd.CommandText =
            """
            UPDATE MediaAttachments
            SET Approved = 0,
                Rejected = 1
            WHERE Id = $id;
            """;

        cmd.Parameters.AddWithValue("$id", mediaId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<ApprovedPostResultDto?> GetRandomApprovedAsync()
    {
        await using var conn = database.CreateConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();

        cmd.CommandText =
            """
            SELECT

                m.Id,
                m.Type,
                m.Url,
                m.PreviewUrl,
                m.RemoteUrl,

                p.Id,
                p.Url,

                a.Id,
                a.Username,
                a.Acct,
                a.DisplayName,
                a.IsBot,
                a.Url,
                a.AvatarStatic

            FROM MediaAttachments m

            INNER JOIN Posts p
                ON p.Id = m.PostId

            INNER JOIN Accounts a
                ON a.Id = p.AccountId

            WHERE
                m.Approved = 1
                AND m.Rejected = 0

            ORDER BY RANDOM()
            LIMIT 1;
            """;

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var media = new MediaAttachment
        {
            Id = reader.GetString(0),
            Type = reader.GetString(1),
            Url = reader.GetString(2),
            PreviewUrl = reader.GetString(3),
            RemoteUrl = reader.IsDBNull(4) ? "" : reader.GetString(4)
        };

        var account = new Account
        {
            Id = reader.GetString(7),
            Username = reader.GetString(8),
            Acct = reader.GetString(9),
            DisplayName = reader.GetString(10),
            IsBot = reader.GetBoolean(11),
            Url = reader.GetString(12),
            AvatarStatic = reader.GetString(13)
        };

        var post = new Post
        {
            Id = reader.GetString(5),
            Url = reader.GetString(6),
            Account = account,
            MediaAttachments = [media]
        };

        return new ApprovedPostResultDto(
            post,
            account,
            media
        );
    }
}