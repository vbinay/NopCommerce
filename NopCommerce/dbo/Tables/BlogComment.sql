CREATE TABLE [dbo].[BlogComment] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [CustomerId]   INT            NOT NULL,
    [CommentText]  NVARCHAR (MAX) NULL,
    [BlogPostId]   INT            NOT NULL,
    [CreatedOnUtc] DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [BlogComment_BlogPost] FOREIGN KEY ([BlogPostId]) REFERENCES [dbo].[BlogPost] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [BlogComment_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_BlogComment_BlogPostId]
    ON [dbo].[BlogComment]([BlogPostId] ASC);

