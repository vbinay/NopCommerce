CREATE TABLE [dbo].[OrderNote] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [OrderId]           INT            NOT NULL,
    [Note]              NVARCHAR (MAX) NOT NULL,
    [DownloadId]        INT            NOT NULL,
    [DisplayToCustomer] BIT            NOT NULL,
    [CreatedOnUtc]      DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [OrderNote_Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_OrderNote_OrderId]
    ON [dbo].[OrderNote]([OrderId] ASC);

