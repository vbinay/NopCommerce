CREATE TABLE [dbo].[ShippingByWeight] (
    [Id]                       INT             IDENTITY (1, 1) NOT NULL,
    [StoreId]                  INT             NOT NULL,
    [WarehouseId]              INT             NOT NULL,
    [CountryId]                INT             NOT NULL,
    [StateProvinceId]          INT             NOT NULL,
    [Zip]                      NVARCHAR (400)  NULL,
    [ShippingMethodId]         INT             NOT NULL,
    [From]                     DECIMAL (18, 2) NOT NULL,
    [To]                       DECIMAL (18, 2) NOT NULL,
    [AdditionalFixedCost]      DECIMAL (18, 2) NOT NULL,
    [PercentageRateOfSubtotal] DECIMAL (18, 2) NOT NULL,
    [RatePerWeightUnit]        DECIMAL (18, 2) NOT NULL,
    [LowerWeightLimit]         DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

