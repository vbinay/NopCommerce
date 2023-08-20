CREATE TABLE [dbo].[ShippingMethodRestrictions] (
    [ShippingMethod_Id] INT NOT NULL,
    [Country_Id]        INT NOT NULL,
    PRIMARY KEY CLUSTERED ([ShippingMethod_Id] ASC, [Country_Id] ASC),
    CONSTRAINT [ShippingMethod_RestrictedCountries_Source] FOREIGN KEY ([ShippingMethod_Id]) REFERENCES [dbo].[ShippingMethod] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [ShippingMethod_RestrictedCountries_Target] FOREIGN KEY ([Country_Id]) REFERENCES [dbo].[Country] ([Id]) ON DELETE CASCADE
);

