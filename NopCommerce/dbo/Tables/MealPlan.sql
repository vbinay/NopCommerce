--- #region SODMYWAY-2957
CREATE TABLE [dbo].[MealPlan] (
    [Id]                       INT            IDENTITY (1, 1) NOT NULL,
    [PurchasedWithOrderItemId] INT            NOT NULL,
    [RecipientName]            NVARCHAR (100) NULL,
    [RecipientAddress]         NVARCHAR (400) NULL,
    [RecipientPhone]           NVARCHAR (50)  NULL,
    [RecipientEmail]           NVARCHAR (200) NULL,
    [RecipientAcctNum]         NVARCHAR (100) NULL,
    [IsProcessed]              BIT            NOT NULL,
    [CreatedOnUtc]             DATETIME       NOT NULL,
    [CardAccessRecordId]       INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [MealPlan_PurchasedWithOrderItem] FOREIGN KEY ([PurchasedWithOrderItemId]) REFERENCES [dbo].[OrderItem] ([Id])
);

--- #endregion