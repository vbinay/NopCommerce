CREATE TABLE [dbo].[GoogleProduct] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [ProductId]   INT            NOT NULL,
    [Taxonomy]    NVARCHAR (MAX) NULL,
    [CustomGoods] BIT            NOT NULL,
    [Gender]      NVARCHAR (MAX) NULL,
    [AgeGroup]    NVARCHAR (MAX) NULL,
    [Color]       NVARCHAR (MAX) NULL,
    [Size]        NVARCHAR (MAX) NULL,
    [Material]    NVARCHAR (MAX) NULL,
    [Pattern]     NVARCHAR (MAX) NULL,
    [ItemGroupId] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

