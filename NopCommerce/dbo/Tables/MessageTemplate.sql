CREATE TABLE [dbo].[MessageTemplate] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [Name]               NVARCHAR (200)  NOT NULL,
    [BccEmailAddresses]  NVARCHAR (200)  NULL,
    [Subject]            NVARCHAR (1000) NULL,
    [Body]               NVARCHAR (MAX)  NULL,
    [IsActive]           BIT             NOT NULL,
    [DelayBeforeSend]    INT             NULL,	--- SODMYWAY-3297
    [DelayPeriodId]      INT             NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    [AttachedDownloadId] INT             NOT NULL,
    [EmailAccountId]     INT             NOT NULL,
    [LimitedToStores]    BIT             NOT NULL,
    [IsMaster]           BIT             NULL,	--- SODMYWAY-2945
    [MasterId]           INT             NULL,	--- SODMYWAY-2945
    [EmailTypeId]        INT             NULL,	--- SODMYWAY-2949
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);



