CREATE TABLE [dbo].[RelatedProduct] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [ProductId1]   INT NOT NULL,
    [ProductId2]   INT NOT NULL,
    [DisplayOrder] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_RelatedProduct_ProductId1]
    ON [dbo].[RelatedProduct]([ProductId1] ASC);

