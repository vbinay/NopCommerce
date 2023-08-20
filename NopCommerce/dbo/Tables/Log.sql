CREATE TABLE [dbo].[Log] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [LogLevelId]   INT            NOT NULL,
    [ShortMessage] NVARCHAR (MAX) NOT NULL,
    [FullMessage]  NVARCHAR (MAX) NULL,
    [IpAddress]    NVARCHAR (200) NULL,
    [CustomerId]   INT            NULL,
    [PageUrl]      NVARCHAR (MAX) NULL,
    [ReferrerUrl]  NVARCHAR (MAX) NULL,
    [CreatedOnUtc] DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Log_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Log_CreatedOnUtc]
    ON [dbo].[Log]([CreatedOnUtc] ASC);

