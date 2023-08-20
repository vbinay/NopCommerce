CREATE TABLE [dbo].[ProductAttributeCombination] (
    [Id]                          INT             IDENTITY (1, 1) NOT NULL,
    [ProductId]                   INT             NOT NULL,
    [AttributesXml]               NVARCHAR (MAX)  NULL,
    [StockQuantity]               INT             NOT NULL,
    [AllowOutOfStockOrders]       BIT             NOT NULL,
    [Sku]                         NVARCHAR (400)  NULL,
    [ManufacturerPartNumber]      NVARCHAR (400)  NULL,
    [Gtin]                        NVARCHAR (400)  NULL,
    [OverriddenPrice]             DECIMAL (18, 4) NULL,
    [NotifyAdminForQuantityBelow] INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductAttributeCombination_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);

