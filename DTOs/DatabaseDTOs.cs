using CatsOfMastodonBot.Models;

namespace CatsOfMastodonBot.DTOs;

public sealed record ApprovedPostResultDto(
    Post Post,
    Account Account,
    MediaAttachment MediaAttachment
);