CREATE TABLE [dbo].[GiftCardUsageHistory] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [GiftCardId]      INT             NOT NULL,
    [UsedWithOrderId] INT             NOT NULL,
    [UsedValue]       DECIMAL (18, 4) NOT NULL,
    [CreatedOnUtc]    DATETIME        NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [GiftCardUsageHistory_GiftCard] FOREIGN KEY ([GiftCardId]) REFERENCES [dbo].[GiftCard] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [GiftCardUsageHistory_UsedWithOrder] FOREIGN KEY ([UsedWithOrderId]) REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE
);

