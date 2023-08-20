CREATE TABLE [dbo].[Product_Category_Mapping] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [ProductId]         INT NOT NULL,
    [CategoryId]        INT NOT NULL,
    [IsFeaturedProduct] BIT NOT NULL,
    [DisplayOrder]      INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductCategory_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductCategory_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PCM_Product_and_Category]
    ON [dbo].[Product_Category_Mapping]([CategoryId] ASC, [ProductId] ASC);

