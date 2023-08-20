--- #region SODMYWAY-2957
CREATE TABLE [dbo].[Product_GLSpecificationAttribute_Mapping] (
    [Id]                               INT             IDENTITY (1, 1) NOT NULL,
    [ProductId]                        INT             NOT NULL,
    [AttributeTypeId]                  INT             NOT NULL,
    [GLSpecificationAttributeOptionId] INT             NOT NULL,
    [CustomValue]                      NVARCHAR (4000) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [ProductGLSpecificationAttribute_GLSpecificationAttributeOption] FOREIGN KEY ([GLSpecificationAttributeOptionId]) REFERENCES [dbo].[GLSpecificationAttributeOption] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductGLSpecificationAttribute_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);

--- #endregion