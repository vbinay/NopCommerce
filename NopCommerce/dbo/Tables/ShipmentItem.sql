CREATE TABLE [dbo].[ShipmentItem] (
    [Id]          INT IDENTITY (1, 1) NOT NULL,
    [ShipmentId]  INT NOT NULL,
    [OrderItemId] INT NOT NULL,
    [Quantity]    INT NOT NULL,
    [WarehouseId] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ShipmentItem_Shipment] FOREIGN KEY ([ShipmentId]) REFERENCES [dbo].[Shipment] ([Id]) ON DELETE CASCADE
);

