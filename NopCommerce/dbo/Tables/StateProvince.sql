CREATE TABLE [dbo].[StateProvince] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [CountryId]    INT            NOT NULL,
    [Name]         NVARCHAR (100) NOT NULL,
    [Abbreviation] NVARCHAR (100) NULL,
    [Published]    BIT            NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [StateProvince_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_StateProvince_CountryId]
    ON [dbo].[StateProvince]([CountryId] ASC)
    INCLUDE([DisplayOrder]);

