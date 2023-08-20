CREATE TABLE [dbo].[ExternalAuthenticationRecord] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [CustomerId]                INT            NOT NULL,
    [Email]                     NVARCHAR (MAX) NULL,
    [ExternalIdentifier]        NVARCHAR (MAX) NULL,
    [ExternalDisplayIdentifier] NVARCHAR (MAX) NULL,
    [OAuthToken]                NVARCHAR (MAX) NULL,
    [OAuthAccessToken]          NVARCHAR (MAX) NULL,
    [ProviderSystemName]        NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ExternalAuthenticationRecord_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE
);

