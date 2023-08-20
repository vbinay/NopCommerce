--- #region SODMYWAY-2957
CREATE TABLE [dbo].[BitShift_FirstData_StoreLicenseKey_1] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [LicenseKey] NVARCHAR (1024) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion