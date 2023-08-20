CREATE TABLE [dbo].[OrderItem] (
    [Id]                                INT              IDENTITY (1, 1) NOT NULL,
    [OrderItemGuid]                     UNIQUEIDENTIFIER NOT NULL,
    [OrderId]                           INT              NOT NULL,
    [ProductId]                         INT              NOT NULL,
    [Quantity]                          INT              NOT NULL,
    [UnitPriceInclTax]                  DECIMAL (18, 4)  NOT NULL,
    [UnitPriceExclTax]                  DECIMAL (18, 4)  NOT NULL,
    [PriceInclTax]                      DECIMAL (18, 4)  NOT NULL,
    [PriceExclTax]                      DECIMAL (18, 4)  NOT NULL,
    [DiscountAmountInclTax]             DECIMAL (18, 4)  NOT NULL,
    [DiscountAmountExclTax]             DECIMAL (18, 4)  NOT NULL,
    [OriginalProductCost]               DECIMAL (18, 4)  NOT NULL,
    [AttributeDescription]              NVARCHAR (MAX)   NULL,
    [AttributesXml]                     NVARCHAR (MAX)   NULL,
    [DownloadCount]                     INT              NOT NULL,
    [IsDownloadActivated]               BIT              NOT NULL,
    [LicenseDownloadId]                 INT              NULL,
    [ItemWeight]                        DECIMAL (18, 4)  NULL,
    [RentalStartDateUtc]                DATETIME         NULL,
    [RentalEndDateUtc]                  DATETIME         NULL,
    [RequestedFulfillmentDateTimeLocal] DATETIME         NULL,
    [RequestedFullfilmentDateTimeLocal] DATETIME         NULL,
    [StoreCommission]                   DECIMAL (18, 4)  NULL,	--- SODMYWAY-2954
    [SelectedFulfillmentWarehouseId]    INT              CONSTRAINT [DF_OrderItem_SelectedFulfillmentWarehouseId] DEFAULT ((0)) NULL,	--- SODMYWAY-2957
    [RequestedFulfillmentDateTime]      DATETIME         NULL,	--- SODMYWAY-2957
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [OrderItem_Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [OrderItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_OrderItem_OrderId]
    ON [dbo].[OrderItem]([OrderId] ASC);

