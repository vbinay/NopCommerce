CREATE TABLE [dbo].[ShippingMethod] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (400) NOT NULL,
    [Description]  NVARCHAR (MAX) NULL,
    [DisplayOrder] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

