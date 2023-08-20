CREATE TABLE [dbo].[ProductReview] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [CustomerId]      INT            NOT NULL,
    [ProductId]       INT            NOT NULL,
    [StoreId]         INT            NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    [IsApproved]      BIT            NOT NULL,
    [Title]           NVARCHAR (MAX) NULL,
    [ReviewText]      NVARCHAR (MAX) NULL,
    [Rating]          INT            NOT NULL,
    [HelpfulYesTotal] INT            NOT NULL,
    [HelpfulNoTotal]  INT            NOT NULL,
    [CreatedOnUtc]    DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductReview_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductReview_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductReview_Store] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Store] ([Id]) ON DELETE CASCADE	--- SODMYWAY-3297
);


GO
CREATE NONCLUSTERED INDEX [IX_ProductReview_ProductId]
    ON [dbo].[ProductReview]([ProductId] ASC);

