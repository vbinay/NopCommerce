CREATE TABLE [dbo].[Campaign] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (MAX) NOT NULL,
    [Subject]      NVARCHAR (MAX) NOT NULL,
    [Body]         NVARCHAR (MAX) NOT NULL,
    [StoreId]      INT            NOT NULL,
    [CustomerRoleId]        INT            NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    [CreatedOnUtc] DATETIME       NOT NULL,
    [DontSendBeforeDateUtc] DATETIME       NULL,	--- SODMYWAY-3297
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

