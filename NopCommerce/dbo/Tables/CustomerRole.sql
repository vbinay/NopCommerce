CREATE TABLE [dbo].[CustomerRole] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [Name]                   NVARCHAR (255) NOT NULL,
    [FreeShipping]           BIT            NOT NULL,
    [TaxExempt]              BIT            NOT NULL,
    [Active]                 BIT            NOT NULL,
    [IsSystemRole]           BIT            NOT NULL,
    [SystemName]             NVARCHAR (255) NULL,
    [PurchasedWithProductId] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

