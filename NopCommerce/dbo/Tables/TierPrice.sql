CREATE TABLE [dbo].[TierPrice] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [ProductId]      INT             NOT NULL,
    [StoreId]        INT             NOT NULL,
    [CustomerRoleId] INT             NULL,
    [Quantity]       INT             NOT NULL,
    [Price]          DECIMAL (18, 4) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [TierPrice_CustomerRole] FOREIGN KEY ([CustomerRoleId]) REFERENCES [dbo].[CustomerRole] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [TierPrice_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TierPrice_ProductId]
    ON [dbo].[TierPrice]([ProductId] ASC);

