--- #region SODMYWAY-2957
CREATE TABLE [dbo].[BitShift_FirstData_SavedCard] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [Customer_Id]       INT            NOT NULL,
    [BillingAddress_Id] INT            NOT NULL,
    [CardholderName]    NVARCHAR (256) NULL,
    [Token]             NVARCHAR (64)  NULL,
    [ExpireMonth]       INT            NOT NULL,
    [ExpireYear]        INT            NOT NULL,
    [CardType]          NVARCHAR (64)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);
--- #endregion