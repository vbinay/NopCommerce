CREATE TABLE [dbo].[Product_ProductTag_Mapping] (
    [Product_Id]    INT NOT NULL,
    [ProductTag_Id] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Product_Id] ASC, [ProductTag_Id] ASC),
    CONSTRAINT [Product_ProductTags_Source] FOREIGN KEY ([Product_Id]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Product_ProductTags_Target] FOREIGN KEY ([ProductTag_Id]) REFERENCES [dbo].[ProductTag] ([Id]) ON DELETE CASCADE
);

