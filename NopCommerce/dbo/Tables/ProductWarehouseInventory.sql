CREATE TABLE [dbo].[ProductWarehouseInventory] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [ProductId]        INT NOT NULL,
    [WarehouseId]      INT NOT NULL,
    [StockQuantity]    INT NOT NULL,
    [ReservedQuantity] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductWarehouseInventory_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductWarehouseInventory_Warehouse] FOREIGN KEY ([WarehouseId]) REFERENCES [dbo].[Warehouse] ([Id]) ON DELETE CASCADE
);

