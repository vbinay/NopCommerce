CREATE TABLE [dbo].[Forums_Forum] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [ForumGroupId]       INT            NOT NULL,
    [Name]               NVARCHAR (200) NOT NULL,
    [Description]        NVARCHAR (MAX) NULL,
    [NumTopics]          INT            NOT NULL,
    [NumPosts]           INT            NOT NULL,
    [LastTopicId]        INT            NOT NULL,
    [LastPostId]         INT            NOT NULL,
    [LastPostCustomerId] INT            NOT NULL,
    [LastPostTime]       DATETIME       NULL,
    [DisplayOrder]       INT            NOT NULL,
    [CreatedOnUtc]       DATETIME       NOT NULL,
    [UpdatedOnUtc]       DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Forum_ForumGroup] FOREIGN KEY ([ForumGroupId]) REFERENCES [dbo].[Forums_Group] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Forum_ForumGroupId]
    ON [dbo].[Forums_Forum]([ForumGroupId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Forum_DisplayOrder]
    ON [dbo].[Forums_Forum]([DisplayOrder] ASC);

