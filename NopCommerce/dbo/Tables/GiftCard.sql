CREATE TABLE [dbo].[GiftCard] (
    [Id]                       INT             IDENTITY (1, 1) NOT NULL,
    [PurchasedWithOrderItemId] INT             NULL,
    [GiftCardTypeId]           INT             NOT NULL,
    [Amount]                   DECIMAL (18, 4) NOT NULL,
    [IsGiftCardActivated]      BIT             NOT NULL,
    [GiftCardCouponCode]       NVARCHAR (MAX)  NULL,
    [RecipientName]            NVARCHAR (MAX)  NULL,
    [RecipientEmail]           NVARCHAR (MAX)  NULL,
    [SenderName]               NVARCHAR (MAX)  NULL,
    [SenderEmail]              NVARCHAR (MAX)  NULL,
    [Message]                  NVARCHAR (MAX)  NULL,
    [IsRecipientNotified]      BIT             NOT NULL,
    [CreatedOnUtc]             DATETIME        NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [GiftCard_PurchasedWithOrderItem] FOREIGN KEY ([PurchasedWithOrderItemId]) REFERENCES [dbo].[OrderItem] ([Id])
);

