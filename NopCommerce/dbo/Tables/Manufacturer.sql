CREATE TABLE [dbo].[Manufacturer] (
    [Id]                             INT            IDENTITY (1, 1) NOT NULL,
    [Name]                           NVARCHAR (400) NOT NULL,
    [Description]                    NVARCHAR (MAX) NULL,
    [ManufacturerTemplateId]         INT            NOT NULL,
    [MetaKeywords]                   NVARCHAR (400) NULL,
    [MetaDescription]                NVARCHAR (MAX) NULL,
    [MetaTitle]                      NVARCHAR (400) NULL,
    [PictureId]                      INT            NOT NULL,
    [PageSize]                       INT            NOT NULL,
    [AllowCustomersToSelectPageSize] BIT            NOT NULL,
    [PageSizeOptions]                NVARCHAR (200) NULL,
    [PriceRanges]                    NVARCHAR (400) NULL,
    [SubjectToAcl]                   BIT            NOT NULL,
    [LimitedToStores]                BIT            NOT NULL,
    [Published]                      BIT            NOT NULL,
    [Deleted]                        BIT            NOT NULL,
    [DisplayOrder]                   INT            NOT NULL,
    [CreatedOnUtc]                   DATETIME       NOT NULL,
    [UpdatedOnUtc]                   DATETIME       NOT NULL,
    [IsMaster]                       BIT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    [MasterId]                       INT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Manufacturer_SubjectToAcl]
    ON [dbo].[Manufacturer]([SubjectToAcl] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Manufacturer_LimitedToStores]
    ON [dbo].[Manufacturer]([LimitedToStores] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Manufacturer_DisplayOrder]
    ON [dbo].[Manufacturer]([DisplayOrder] ASC);

