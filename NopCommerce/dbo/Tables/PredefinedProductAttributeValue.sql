CREATE TABLE [dbo].[PredefinedProductAttributeValue] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [ProductAttributeId] INT             NOT NULL,
    [Name]               NVARCHAR (400)  NOT NULL,
    [PriceAdjustment]    DECIMAL (18, 4) NOT NULL,
    [WeightAdjustment]   DECIMAL (18, 4) NOT NULL,
    [Cost]               DECIMAL (18, 4) NOT NULL,
    [IsPreSelected]      BIT             NOT NULL,
    [DisplayOrder]       INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [PredefinedProductAttributeValue_ProductAttribute] FOREIGN KEY ([ProductAttributeId]) REFERENCES [dbo].[ProductAttribute] ([Id]) ON DELETE CASCADE
);

