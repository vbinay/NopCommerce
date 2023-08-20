CREATE TABLE [dbo].[Product_Manufacturer_Mapping] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [ProductId]         INT NOT NULL,
    [ManufacturerId]    INT NOT NULL,
    [IsFeaturedProduct] BIT NOT NULL,
    [DisplayOrder]      INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductManufacturer_Manufacturer] FOREIGN KEY ([ManufacturerId]) REFERENCES [dbo].[Manufacturer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductManufacturer_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PMM_Product_and_Manufacturer]
    ON [dbo].[Product_Manufacturer_Mapping]([ManufacturerId] ASC, [ProductId] ASC);

