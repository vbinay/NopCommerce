CREATE TABLE [dbo].[PermissionRecord] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Name]       NVARCHAR (MAX) NOT NULL,
    [SystemName] NVARCHAR (255) NOT NULL,
    [Category]   NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

