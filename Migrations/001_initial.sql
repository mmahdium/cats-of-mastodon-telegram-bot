CREATE TABLE IF NOT EXISTS SchemaMigrations
(
    Version
    TEXT
    PRIMARY
    KEY
);

CREATE TABLE IF NOT EXISTS Accounts
(
    Id
    TEXT
    PRIMARY
    KEY,
    Username
    TEXT
    NOT
    NULL,
    Acct
    TEXT
    NOT
    NULL
    UNIQUE,
    DisplayName
    TEXT
    NOT
    NULL,
    IsBot
    INTEGER
    NOT
    NULL,
    Url
    TEXT
    NOT
    NULL,
    AvatarStatic
    TEXT
    NOT
    NULL
);

CREATE INDEX IF NOT EXISTS IX_Account_Username
    ON Accounts(Username);

CREATE TABLE IF NOT EXISTS Posts
(
    Id
    TEXT
    PRIMARY
    KEY,
    Url
    TEXT
    NOT
    NULL,

    AccountId
    TEXT
    NOT
    NULL,

    FOREIGN
    KEY
(
    AccountId
)
    REFERENCES Accounts
(
    Id
)
    ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS MediaAttachments
(
    Id
    TEXT
    PRIMARY
    KEY,

    Type
    TEXT
    NOT
    NULL,
    Url
    TEXT
    NOT
    NULL,
    PreviewUrl
    TEXT
    NOT
    NULL,
    RemoteUrl
    TEXT,

    Approved
    INTEGER
    NOT
    NULL
    DEFAULT
    0,
    Rejected
    INTEGER
    NOT
    NULL
    DEFAULT
    0,

    PostId
    TEXT,

    FOREIGN
    KEY
(
    PostId
)
    REFERENCES Posts
(
    Id
)
    ON DELETE CASCADE
    );

CREATE INDEX IF NOT EXISTS IX_Media_ApprovalStatus
    ON MediaAttachments(Approved, Rejected);