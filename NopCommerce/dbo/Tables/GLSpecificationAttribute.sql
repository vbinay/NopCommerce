--- #region SODMYWAY-2957
CREATE TABLE [dbo].[GLSpecificationAttribute] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (MAX) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion