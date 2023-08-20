--- #region SODMYWAY-2957
CREATE TABLE [dbo].[VendorMapping] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [EntityId]   INT            NOT NULL,
    [EntityName] NVARCHAR (400) NOT NULL,
    [VendorId]   INT            NOT NULL,
    CONSTRAINT [PK_VendorMapping] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion