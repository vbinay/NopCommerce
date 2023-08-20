CREATE TABLE [dbo].[Discount_AppliedToManufacturers] (
    [Discount_Id]     INT NOT NULL,
    [Manufacturer_Id] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Discount_Id] ASC, [Manufacturer_Id] ASC),
    CONSTRAINT [Discount_AppliedToManufacturers_Source] FOREIGN KEY ([Discount_Id]) REFERENCES [dbo].[Discount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Discount_AppliedToManufacturers_Target] FOREIGN KEY ([Manufacturer_Id]) REFERENCES [dbo].[Manufacturer] ([Id]) ON DELETE CASCADE
);

