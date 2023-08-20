--- #region SODMYWAY-2957
CREATE TABLE [dbo].[GLCode] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [Description]       NVARCHAR (100) NULL,
    [ProductCategoryId] INT            NULL,
    [IsPaid]            BIT            NULL,
    [IsDelievered]      BIT            NULL,
    [GlCode]            VARCHAR (50)   NULL
);

--- #endregion