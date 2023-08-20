--- #region SODMYWAY-2957
CREATE TABLE [dbo].[Donation] (
    [Id]                       INT             IDENTITY (1, 1) NOT NULL,
    [PurchasedWithOrderItemId] INT             NOT NULL,
    [Amount]                   DECIMAL (18, 4) NOT NULL,
    [DonorFirstName]           NVARCHAR (100)  NULL,
    [DonorLastName]            NVARCHAR (100)  NULL,
    [DonorAddress]             NVARCHAR (400)  NULL,
    [DonorAddress2]            NVARCHAR (400)  NULL,
    [DonorCity]                NVARCHAR (100)  NULL,
    [DonorState]               NVARCHAR (100)  NULL,
    [DonorZip]                 NVARCHAR (100)  NULL,
    [DonorPhone]               NVARCHAR (100)  NULL,
    [DonorCountry]             NVARCHAR (100)  NULL,
    [Comments]                 NVARCHAR (400)  NULL,
    [NotificationEmail]        NVARCHAR (100)  NULL,
    [OnBehalfOfFullName]       NVARCHAR (200)  NULL,
    [IsProcessed]              BIT             NOT NULL,
    [IncludeGiftAmount]        BIT             NOT NULL,
    [CreatedOnUtc]             DATETIME        NOT NULL,
    [DonorCompany]             NVARCHAR (100)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [Donation_PurchasedWithOrderItem] FOREIGN KEY ([PurchasedWithOrderItemId]) REFERENCES [dbo].[OrderItem] ([Id])
);

--- #endregion