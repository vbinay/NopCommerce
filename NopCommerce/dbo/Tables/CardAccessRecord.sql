--- #region SODMYWAY-2957
CREATE TABLE [dbo].[CardAccessRecord] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [CustomerId]    INT            NULL,
    [CardHolderId]  NVARCHAR (MAX) NULL,
    [PlanId]        NVARCHAR (MAX) NULL,
    [IssuerId]      NVARCHAR (MAX) NULL,
    [ApplicationID] NVARCHAR (MAX) NULL,
    [Type]          NVARCHAR (3)   NULL,
    [AccountId]     NVARCHAR (MAX) NULL,
    [Balance]       NVARCHAR (200) NULL,
    [Status]        NVARCHAR (MAX) NULL,
    [HashUsed]      NVARCHAR (MAX) NULL,
    [CreatedOnUtc]  DATETIME       NULL,
    [ReferenceID]   NVARCHAR (MAX) NULL,
    [AppliedAmount] NVARCHAR (MAX) NULL,
    [Hash]          NVARCHAR (MAX) NULL,
    [Error]         NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

--- #endregion