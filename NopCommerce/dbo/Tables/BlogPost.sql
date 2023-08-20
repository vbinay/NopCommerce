CREATE TABLE [dbo].[BlogPost] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [LanguageId]      INT            NOT NULL,
    [Title]           NVARCHAR (MAX) NOT NULL,
    [Body]            NVARCHAR (MAX) NOT NULL,
    [BodyOverview]    NVARCHAR (MAX) NULL,
    [AllowComments]   BIT            NOT NULL,
    [CommentCount]    INT            NOT NULL,
    [Tags]            NVARCHAR (MAX) NULL,
    [StartDateUtc]    DATETIME       NULL,
    [EndDateUtc]      DATETIME       NULL,
    [MetaKeywords]    NVARCHAR (400) NULL,
    [MetaDescription] NVARCHAR (MAX) NULL,
    [MetaTitle]       NVARCHAR (400) NULL,
    [LimitedToStores] BIT            NOT NULL,
    [CreatedOnUtc]    DATETIME       NOT NULL,
    [IsMaster]        BIT            NULL,	--- SODMYWAY-2945
    [MasterId]        INT            NULL,	--- SODMYWAY-2945
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [BlogPost_Language] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_BlogPost_LanguageId]
    ON [dbo].[BlogPost]([LanguageId] ASC);

