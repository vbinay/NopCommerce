--- #region SODMYWAY-2957
CREATE TABLE [dbo].[GLSpecificationAttributeOption] (
    [Id]                         INT            IDENTITY (1, 1) NOT NULL,
    [GLSpecificationAttributeId] INT            NOT NULL,
    [Name]                       NVARCHAR (MAX) NOT NULL,
    [DisplayOrder]               INT            NOT NULL,
    [Value]                      NVARCHAR (MAX) NULL,
    [Amount]                     NVARCHAR (MAX) NULL,
    [IsDeferred]                 BIT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [GLSpecificationAttributeOption_GLSpecificationAttribute] FOREIGN KEY ([GLSpecificationAttributeId]) REFERENCES [dbo].[GLSpecificationAttribute] ([Id]) ON DELETE CASCADE
);

--- #endregion