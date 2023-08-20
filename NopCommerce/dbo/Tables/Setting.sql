CREATE TABLE [dbo].[Setting] (
    [Id]      INT             IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (200)  NOT NULL,
    [Value]   NVARCHAR (2000) NOT NULL,
    [StoreId] INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

