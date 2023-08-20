CREATE TABLE [dbo].[Customer] (
    [Id]                   INT              IDENTITY (1, 1) NOT NULL,
    [CustomerGuid]         UNIQUEIDENTIFIER NOT NULL,
    [Username]             NVARCHAR (1000)  NULL,
    [Email]                NVARCHAR (1000)  NULL,
    [Password]             NVARCHAR (MAX)   NULL,
    [PasswordFormatId]     INT              NOT NULL,
    [PasswordSalt]         NVARCHAR (MAX)   NULL,
    [AdminComment]         NVARCHAR (MAX)   NULL,
    [IsTaxExempt]          BIT              NOT NULL,
    [AffiliateId]          INT              NOT NULL,
    [VendorId]             INT              NOT NULL,
    [HasShoppingCartItems] BIT              NOT NULL,
    [Active]               BIT              NOT NULL,
    [Deleted]              BIT              NOT NULL,
    [IsSystemAccount]      BIT              NOT NULL,
    [SystemName]           NVARCHAR (400)   NULL,
    [LastIpAddress]        NVARCHAR (MAX)   NULL,
    [CreatedOnUtc]         DATETIME         NOT NULL,
    [LastLoginDateUtc]     DATETIME         NULL,
    [LastActivityDateUtc]  DATETIME         NOT NULL,
    [BillingAddress_Id]    INT              NULL,
    [ShippingAddress_Id]   INT              NULL,
    [LimitedToStores]      BIT              NULL,	--- SODMYWAY-2946
    [IsMaster]             BIT              DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    [MasterId]             INT              DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Customer_BillingAddress] FOREIGN KEY ([BillingAddress_Id]) REFERENCES [dbo].[Address] ([Id]),
    CONSTRAINT [Customer_ShippingAddress] FOREIGN KEY ([ShippingAddress_Id]) REFERENCES [dbo].[Address] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Customer_SystemName]
    ON [dbo].[Customer]([SystemName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Customer_CustomerGuid]
    ON [dbo].[Customer]([CustomerGuid] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Customer_Username]
    ON [dbo].[Customer]([Username] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Customer_Email]
    ON [dbo].[Customer]([Email] ASC);

