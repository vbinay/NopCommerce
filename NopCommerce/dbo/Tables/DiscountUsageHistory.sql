CREATE TABLE [dbo].[DiscountUsageHistory] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [DiscountId]   INT      NOT NULL,
    [OrderId]      INT      NOT NULL,
    [CreatedOnUtc] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [DiscountUsageHistory_Discount] FOREIGN KEY ([DiscountId]) REFERENCES [dbo].[Discount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [DiscountUsageHistory_Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE
);

