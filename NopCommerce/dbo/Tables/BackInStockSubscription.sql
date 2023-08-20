CREATE TABLE [dbo].[BackInStockSubscription] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [StoreId]      INT      NOT NULL,
    [ProductId]    INT      NOT NULL,
    [CustomerId]   INT      NOT NULL,
    [CreatedOnUtc] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [BackInStockSubscription_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [BackInStockSubscription_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);

