--- #region SODMYWAY-2957
CREATE TABLE [dbo].[BitShift_FirstData_StoreSetting] (
    [StoreId]                   INT            NOT NULL,
    [UseSandbox]                BIT            NOT NULL,
    [TransactionMode]           INT            NOT NULL,
    [HMAC]                      NVARCHAR (128) NULL,
    [GatewayID]                 NVARCHAR (128) NULL,
    [Password]                  NVARCHAR (128) NULL,
    [KeyID]                     NVARCHAR (128) NULL,
    [EnableRecurringPayments]   BIT            NOT NULL,
    [EnableCardSaving]          BIT            NOT NULL,
    [EnablePurchaseOrderNumber] BIT            NOT NULL,
    [AdditionalFeePercentage]   BIT            NOT NULL,
    [AdditionalFee]             DECIMAL (9, 4) NOT NULL,
    PRIMARY KEY CLUSTERED ([StoreId] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion