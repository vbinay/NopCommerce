CREATE TABLE [dbo].[Product_SpecificationAttribute_Mapping] (
    [Id]                             INT             IDENTITY (1, 1) NOT NULL,
    [ProductId]                      INT             NOT NULL,
    [AttributeTypeId]                INT             NOT NULL,
    [SpecificationAttributeOptionId] INT             NOT NULL,
    [CustomValue]                    NVARCHAR (4000) NULL,
    [AllowFiltering]                 BIT             NOT NULL,
    [ShowOnProductPage]              BIT             NOT NULL,
    [DisplayOrder]                   INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductSpecificationAttribute_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductSpecificationAttribute_SpecificationAttributeOption] FOREIGN KEY ([SpecificationAttributeOptionId]) REFERENCES [dbo].[SpecificationAttributeOption] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PSAM_ProductId]
    ON [dbo].[Product_SpecificationAttribute_Mapping]([ProductId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PSAM_SpecificationAttributeOptionId_AllowFiltering]
    ON [dbo].[Product_SpecificationAttribute_Mapping]([SpecificationAttributeOptionId] ASC, [AllowFiltering] ASC)
    INCLUDE([ProductId]);


GO
CREATE NONCLUSTERED INDEX [IX_PSAM_AllowFiltering]
    ON [dbo].[Product_SpecificationAttribute_Mapping]([AllowFiltering] ASC)
    INCLUDE([ProductId], [SpecificationAttributeOptionId]);

