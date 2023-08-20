CREATE TABLE [dbo].[QueuedEmail] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [CustomNumber]          NVARCHAR (MAX) NULL,	--- SODMYWAY-3297
    [PriorityId]         INT             NOT NULL,
    [From]               NVARCHAR (500)  NOT NULL,
    [FromName]           NVARCHAR (500)  NULL,
    [To]                 NVARCHAR (500)  NOT NULL,
    [ToName]             NVARCHAR (500)  NULL,
    [ReplyTo]            NVARCHAR (500)  NULL,
    [ReplyToName]        NVARCHAR (500)  NULL,
    [CC]                 NVARCHAR (500)  NULL,
    [Bcc]                NVARCHAR (500)  NULL,
    [Subject]            NVARCHAR (1000) NULL,
    [Body]               NVARCHAR (MAX)  NULL,
    [AttachmentFilePath] NVARCHAR (MAX)  NULL,
    [AttachmentFileName] NVARCHAR (MAX)  NULL,
    [AttachedDownloadId] INT             NOT NULL,
    [CreatedOnUtc]       DATETIME        NOT NULL,
    [DontSendBeforeDateUtc] DATETIME        NULL,	--- SODMYWAY-3297
    [SentTries]          INT             NOT NULL,
    [SentOnUtc]          DATETIME        NULL,
    [EmailAccountId]     INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [QueuedEmail_EmailAccount] FOREIGN KEY ([EmailAccountId]) REFERENCES [dbo].[EmailAccount] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_QueuedEmail_CreatedOnUtc]
    ON [dbo].[QueuedEmail]([CreatedOnUtc] ASC);

