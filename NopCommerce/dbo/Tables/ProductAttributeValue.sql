CREATE TABLE [dbo].[ProductAttributeValue] (
    [Id]                        INT             IDENTITY (1, 1) NOT NULL,
    [ProductAttributeMappingId] INT             NOT NULL,
    [AttributeValueTypeId]      INT             NOT NULL,
    [AssociatedProductId]       INT             NOT NULL,
    [Name]                      NVARCHAR (400)  NOT NULL,
    [ColorSquaresRgb]           NVARCHAR (100)  NULL,
    [ImageSquaresPictureId]     INT             NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    [PriceAdjustment]           DECIMAL (18, 4) NOT NULL,
    [WeightAdjustment]          DECIMAL (18, 4) NOT NULL,
    [Cost]                      DECIMAL (18, 4) NOT NULL,
    [Quantity]                  INT             NOT NULL,
    [IsPreSelected]             BIT             NOT NULL,
    [DisplayOrder]              INT             NOT NULL,
    [PictureId]                 INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductAttributeValue_ProductAttributeMapping] FOREIGN KEY ([ProductAttributeMappingId]) REFERENCES [dbo].[Product_ProductAttribute_Mapping] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_ProductAttributeValue_ProductAttributeMappingId_DisplayOrder]
    ON [dbo].[ProductAttributeValue]([ProductAttributeMappingId] ASC, [DisplayOrder] ASC);

