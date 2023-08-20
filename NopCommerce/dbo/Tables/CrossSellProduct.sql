CREATE TABLE [dbo].[CrossSellProduct] (
    [Id]         INT IDENTITY (1, 1) NOT NULL,
    [ProductId1] INT NOT NULL,
    [ProductId2] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

