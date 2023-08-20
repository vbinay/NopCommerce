CREATE TABLE [dbo].[Shipment] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [OrderId]         INT             NOT NULL,
    [TrackingNumber]  NVARCHAR (MAX)  NULL,
    [TotalWeight]     DECIMAL (18, 4) NULL,
    [ShippedDateUtc]  DATETIME        NULL,
    [DeliveryDateUtc] DATETIME        NULL,
    [AdminComment]    NVARCHAR (MAX)  NULL,
    [CreatedOnUtc]    DATETIME        NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Shipment_Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE
);

