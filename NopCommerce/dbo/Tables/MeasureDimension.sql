CREATE TABLE [dbo].[MeasureDimension] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (100)  NOT NULL,
    [SystemKeyword] NVARCHAR (100)  NOT NULL,
    [Ratio]         DECIMAL (18, 8) NOT NULL,
    [DisplayOrder]  INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

