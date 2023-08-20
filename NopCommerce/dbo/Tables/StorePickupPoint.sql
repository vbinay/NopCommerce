--- #region SODMYWAY-3297
CREATE TABLE [dbo].[StorePickupPoint] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (MAX)  NULL,
    [Description]  NVARCHAR (MAX)  NULL,
    [AddressId]    INT             NOT NULL,
    [PickupFee]    DECIMAL (18, 4) NOT NULL,
    [OpeningHours] NVARCHAR (MAX)  NULL,
    [StoreId]      INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

--- #endregion