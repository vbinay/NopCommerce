CREATE TABLE [dbo].[Discount_AppliedToProducts] (
    [Discount_Id] INT NOT NULL,
    [Product_Id]  INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Discount_Id] ASC, [Product_Id] ASC),
    CONSTRAINT [Discount_AppliedToProducts_Source] FOREIGN KEY ([Discount_Id]) REFERENCES [dbo].[Discount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Discount_AppliedToProducts_Target] FOREIGN KEY ([Product_Id]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
);

