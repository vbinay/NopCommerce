CREATE TABLE [dbo].[VendorNote] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [VendorId]     INT            NOT NULL,
    [Note]         NVARCHAR (MAX) NOT NULL,
    [CreatedOnUtc] DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [VendorNote_Vendor] FOREIGN KEY ([VendorId]) REFERENCES [dbo].[Vendor] ([Id]) ON DELETE CASCADE
);

