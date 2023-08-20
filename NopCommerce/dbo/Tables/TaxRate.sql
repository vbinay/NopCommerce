CREATE TABLE [dbo].[TaxRate] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [StoreId]         INT             NOT NULL,
    [TaxCategoryId]   INT             NOT NULL,
    [CountryId]       INT             NOT NULL,
    [StateProvinceId] INT             NOT NULL,
    [Zip]             NVARCHAR (MAX)  NULL,
    [Percentage]      DECIMAL (18, 4) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

