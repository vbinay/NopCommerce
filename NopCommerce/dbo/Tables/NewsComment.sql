CREATE TABLE [dbo].[NewsComment] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [CommentTitle] NVARCHAR (MAX) NULL,
    [CommentText]  NVARCHAR (MAX) NULL,
    [NewsItemId]   INT            NOT NULL,
    [CustomerId]   INT            NOT NULL,
    [CreatedOnUtc] DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [NewsComment_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [NewsComment_NewsItem] FOREIGN KEY ([NewsItemId]) REFERENCES [dbo].[News] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_NewsComment_NewsItemId]
    ON [dbo].[NewsComment]([NewsItemId] ASC);

