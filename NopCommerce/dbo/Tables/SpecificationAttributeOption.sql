CREATE TABLE [dbo].[SpecificationAttributeOption] (
    [Id]                       INT            IDENTITY (1, 1) NOT NULL,
    [SpecificationAttributeId] INT            NOT NULL,
    [Name]                     NVARCHAR (MAX) NOT NULL,
    [ColorSquaresRgb]          NVARCHAR (100) NULL,	--- SODMYWAY-3297
    [DisplayOrder]             INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [SpecificationAttributeOption_SpecificationAttribute] FOREIGN KEY ([SpecificationAttributeId]) REFERENCES [dbo].[SpecificationAttribute] ([Id]) ON DELETE CASCADE
);

