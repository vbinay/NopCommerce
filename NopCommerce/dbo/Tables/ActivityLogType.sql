CREATE TABLE [dbo].[ActivityLogType] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [SystemKeyword] NVARCHAR (100) NOT NULL,
    [Name]          NVARCHAR (200) NOT NULL,
    [Enabled]       BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

