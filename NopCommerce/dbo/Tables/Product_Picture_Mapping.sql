CREATE TABLE [dbo].[Product_Picture_Mapping] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [ProductId]    INT NOT NULL,
    [PictureId]    INT NOT NULL,
    [DisplayOrder] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductPicture_Picture] FOREIGN KEY ([PictureId]) REFERENCES [dbo].[Picture] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ProductPicture_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);

