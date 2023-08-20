CREATE TABLE [dbo].[UrlRecord] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [EntityId]   INT            NOT NULL,
    [EntityName] NVARCHAR (400) NOT NULL,
    [Slug]       NVARCHAR (400) NOT NULL,
    [IsActive]   BIT            NOT NULL,
    [LanguageId] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_UrlRecord_Custom_1]
    ON [dbo].[UrlRecord]([EntityId] ASC, [EntityName] ASC, [LanguageId] ASC, [IsActive] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UrlRecord_Slug]
    ON [dbo].[UrlRecord]([Slug] ASC);

