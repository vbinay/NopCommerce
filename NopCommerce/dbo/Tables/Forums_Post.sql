CREATE TABLE [dbo].[Forums_Post] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [TopicId]      INT            NOT NULL,
    [CustomerId]   INT            NOT NULL,
    [Text]         NVARCHAR (MAX) NOT NULL,
    [IPAddress]    NVARCHAR (100) NULL,
    [CreatedOnUtc] DATETIME       NOT NULL,
    [UpdatedOnUtc] DATETIME       NOT NULL,
    [VoteCount]    INT            NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ForumPost_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]),
    CONSTRAINT [ForumPost_ForumTopic] FOREIGN KEY ([TopicId]) REFERENCES [dbo].[Forums_Topic] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Post_CustomerId]
    ON [dbo].[Forums_Post]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Post_TopicId]
    ON [dbo].[Forums_Post]([TopicId] ASC);

