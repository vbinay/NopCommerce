--- #region SODMYWAY-3297
CREATE TABLE [dbo].[Forums_PostVote] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [ForumPostId]  INT      NOT NULL,
    [CustomerId]   INT      NOT NULL,
    [IsUp]         BIT      NOT NULL,
    [CreatedOnUtc] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ForumPostVote_ForumPost] FOREIGN KEY ([ForumPostId]) REFERENCES [dbo].[Forums_Post] ([Id]) ON DELETE CASCADE
);
GO

--- #endregion