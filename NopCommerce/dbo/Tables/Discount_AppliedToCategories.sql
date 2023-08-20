CREATE TABLE [dbo].[Discount_AppliedToCategories] (
    [Discount_Id] INT NOT NULL,
    [Category_Id] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Discount_Id] ASC, [Category_Id] ASC),
    CONSTRAINT [Discount_AppliedToCategories_Source] FOREIGN KEY ([Discount_Id]) REFERENCES [dbo].[Discount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Discount_AppliedToCategories_Target] FOREIGN KEY ([Category_Id]) REFERENCES [dbo].[Category] ([Id]) ON DELETE CASCADE
);

