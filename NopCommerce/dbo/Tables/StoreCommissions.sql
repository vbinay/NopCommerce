--- #region SODMYWAY-2957
CREATE TABLE [dbo].[StoreCommissions] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [VendorId]   INT             NOT NULL,
    [StoreId]    INT             NOT NULL,
    [Commission] DECIMAL (18, 4) NOT NULL,
    CONSTRAINT [PK_StoreComission] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_StoreComission_Store] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Store] ([Id]),
    CONSTRAINT [FK_StoreComission_Vendor] FOREIGN KEY ([VendorId]) REFERENCES [dbo].[Vendor] ([Id])
);

--- #endregion