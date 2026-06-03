using CatsOfMastodonBot.Models;

namespace CatsOfMastodonBot.DTOs;

public sealed record ApprovedPostResultDto(
    Post Post,
    Account Account,
    MediaAttachment MediaAttachment
);

public sealed record ActionPostResultDto(
    Post Post,
    Account Account,
    MediaAttachment MediaAttachment
);