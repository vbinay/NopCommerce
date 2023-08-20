CREATE TABLE [dbo].[LocaleStringResource] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [LanguageId]    INT            NOT NULL,
    [ResourceName]  NVARCHAR (200) NOT NULL,
    [ResourceValue] NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [LocaleStringResource_Language] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_LocaleStringResource]
    ON [dbo].[LocaleStringResource]([ResourceName] ASC, [LanguageId] ASC);

