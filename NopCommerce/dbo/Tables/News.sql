CREATE TABLE [dbo].[News] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [LanguageId]      INT            NOT NULL,
    [Title]           NVARCHAR (MAX) NOT NULL,
    [Short]           NVARCHAR (MAX) NOT NULL,
    [Full]            NVARCHAR (MAX) NOT NULL,
    [Published]       BIT            NOT NULL,
    [StartDateUtc]    DATETIME       NULL,
    [EndDateUtc]      DATETIME       NULL,
    [AllowComments]   BIT            NOT NULL,
    [CommentCount]    INT            NOT NULL,
    [LimitedToStores] BIT            NOT NULL,
    [MetaKeywords]    NVARCHAR (400) NULL,
    [MetaDescription] NVARCHAR (MAX) NULL,
    [MetaTitle]       NVARCHAR (400) NULL,
    [CreatedOnUtc]    DATETIME       NOT NULL,
    [IsMaster]        BIT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    [MasterId]        INT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [NewsItem_Language] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_News_LanguageId]
    ON [dbo].[News]([LanguageId] ASC);

