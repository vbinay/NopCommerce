CREATE TABLE [dbo].[Address] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]        NVARCHAR (MAX) NULL,
    [LastName]         NVARCHAR (MAX) NULL,
    [Email]            NVARCHAR (MAX) NULL,
    [Company]          NVARCHAR (MAX) NULL,
    [CountryId]        INT            NULL,
    [StateProvinceId]  INT            NULL,
    [City]             NVARCHAR (MAX) NULL,
    [Address1]         NVARCHAR (MAX) NULL,
    [Address2]         NVARCHAR (MAX) NULL,
    [ZipPostalCode]    NVARCHAR (MAX) NULL,
    [PhoneNumber]      NVARCHAR (MAX) NULL,
    [FaxNumber]        NVARCHAR (MAX) NULL,
    [CustomAttributes] NVARCHAR (MAX) NULL,
    [CreatedOnUtc]     DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Address_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [Address_StateProvince] FOREIGN KEY ([StateProvinceId]) REFERENCES [dbo].[StateProvince] ([Id])
);

