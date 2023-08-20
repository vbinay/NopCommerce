--- #region SODMYWAY-2957
CREATE TABLE [dbo].[FulfillmentCalDay] (
    [Id]      INT      IDENTITY (1, 1) NOT NULL,
    [StoreId] INT      NOT NULL,
    [Date]    DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FulfillmentCalDay_Store] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Store] ([Id])
);

--- #endregion