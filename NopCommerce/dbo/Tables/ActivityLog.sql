CREATE TABLE [dbo].[ActivityLog] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [ActivityLogTypeId] INT            NOT NULL,
    [CustomerId]        INT            NOT NULL,
    [Comment]           NVARCHAR (MAX) NOT NULL,
    [CreatedOnUtc]      DATETIME       NOT NULL,
    [IpAddress]         NVARCHAR (200) NULL,	--- SODMYWAY-3297
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ActivityLog_ActivityLogType] FOREIGN KEY ([ActivityLogTypeId]) REFERENCES [dbo].[ActivityLogType] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ActivityLog_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_ActivityLog_CreatedOnUtc]
    ON [dbo].[ActivityLog]([CreatedOnUtc] ASC);

