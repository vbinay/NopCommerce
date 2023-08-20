CREATE TABLE [dbo].[ShoppingCartItem] (
    [Id]                   INT             IDENTITY (1, 1) NOT NULL,
    [StoreId]              INT             NOT NULL,
    [ShoppingCartTypeId]   INT             NOT NULL,
    [CustomerId]           INT             NOT NULL,
    [ProductId]            INT             NOT NULL,
    [AttributesXml]        NVARCHAR (MAX)  NULL,
    [CustomerEnteredPrice] DECIMAL (18, 4) NOT NULL,
    [Quantity]             INT             NOT NULL,
    [RentalStartDateUtc]   DATETIME        NULL,
    [RentalEndDateUtc]     DATETIME        NULL,
    [CreatedOnUtc]         DATETIME        NOT NULL,
    [UpdatedOnUtc]         DATETIME        NOT NULL,
    [RequestedFullfilmentDateTime] DATETIME NULL, 	--- SODMYWAY-2941
    [SelectedWarehouseId] INT NULL, 	--- SODMYWAY-2941
    [SelectedShippingMethodName] NVARCHAR(MAX) NULL, 	--- SODMYWAY-2941
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ShoppingCartItem_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ShoppingCartItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_ShoppingCartItem_ShoppingCartTypeId_CustomerId]
    ON [dbo].[ShoppingCartItem]([ShoppingCartTypeId] ASC, [CustomerId] ASC);

