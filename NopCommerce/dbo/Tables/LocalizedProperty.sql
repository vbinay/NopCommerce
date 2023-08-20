CREATE TABLE [dbo].[LocalizedProperty] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [EntityId]       INT            NOT NULL,
    [LanguageId]     INT            NOT NULL,
    [LocaleKeyGroup] NVARCHAR (400) NOT NULL,
    [LocaleKey]      NVARCHAR (400) NOT NULL,
    [LocaleValue]    NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [LocalizedProperty_Language] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]) ON DELETE CASCADE
);

