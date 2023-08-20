--- #region SODMYWAY-2957
CREATE TABLE [dbo].[TridionProfileMapping] (
    [Id]                          INT            IDENTITY (1, 1) NOT NULL,
    [TridionIdentificationSource] NVARCHAR (MAX) NOT NULL,
    [TridionIdentificationKey]    NVARCHAR (MAX) NOT NULL,
    [CustomerId]                  INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion