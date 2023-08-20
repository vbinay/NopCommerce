--- #region SODMYWAY-2957
CREATE TABLE [dbo].[EmailType] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (50) NOT NULL,
    [Deleted] BIT           NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion