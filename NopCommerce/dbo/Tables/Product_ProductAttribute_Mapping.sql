CREATE TABLE [dbo].[Product_ProductAttribute_Mapping] (
    [Id]                              INT            IDENTITY (1, 1) NOT NULL,
    [ProductId]                       INT            NOT NULL,
    [ProductAttributeId]              INT            NOT NULL,
    [TextPrompt]                      NVARCHAR (MAX) NULL,
    [IsRequired]                      BIT            NOT NULL,
    [AttributeControlTypeId]          INT            NOT NULL,
    [DisplayOrder]                    INT            NOT NULL,
    [ValidationMinLength]             INT            NULL,
    [ValidationMaxLength]             INT            NULL,
    [ValidationFileAllowedExtensions] NVARCHAR (MAX) NULL,
    [ValidationFileMaximumSize]       INT            NULL,
    [DefaultValue]                    NVARCHAR (MAX) NULL,
    [ConditionAttributeXml]           NVARCHAR (MAX) NULL,
    [SiteProductVariantAttributeId]   INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductAttributeMapping_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductAttributeMapping_ProductAttribute] FOREIGN KEY ([ProductAttributeId]) REFERENCES [dbo].[ProductAttribute] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder]
    ON [dbo].[Product_ProductAttribute_Mapping]([ProductId] ASC, [DisplayOrder] ASC);

