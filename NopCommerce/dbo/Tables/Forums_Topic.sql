CREATE TABLE [dbo].[Forums_Topic] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [ForumId]            INT            NOT NULL,
    [CustomerId]         INT            NOT NULL,
    [TopicTypeId]        INT            NOT NULL,
    [Subject]            NVARCHAR (450) NOT NULL,
    [NumPosts]           INT            NOT NULL,
    [Views]              INT            NOT NULL,
    [LastPostId]         INT            NOT NULL,
    [LastPostCustomerId] INT            NOT NULL,
    [LastPostTime]       DATETIME       NULL,
    [CreatedOnUtc]       DATETIME       NOT NULL,
    [UpdatedOnUtc]       DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ForumTopic_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]),
    CONSTRAINT [ForumTopic_Forum] FOREIGN KEY ([ForumId]) REFERENCES [dbo].[Forums_Forum] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Topic_ForumId]
    ON [dbo].[Forums_Topic]([ForumId] ASC);

