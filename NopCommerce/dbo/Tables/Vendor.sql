CREATE TABLE [dbo].[Vendor] (
    [Id]                             INT            IDENTITY (1, 1) NOT NULL,
    [Name]                           NVARCHAR (400) NOT NULL,
    [Email]                          NVARCHAR (400) NULL,
    [Description]                    NVARCHAR (MAX) NULL,
    [PictureId]                      INT            NOT NULL,
    [AdminComment]                   NVARCHAR (MAX) NULL,
    [Active]                         BIT            NOT NULL,
    [Deleted]                        BIT            NOT NULL,
    [DisplayOrder]                   INT            NOT NULL,
    [MetaKeywords]                   NVARCHAR (400) NULL,
    [MetaDescription]                NVARCHAR (MAX) NULL,
    [MetaTitle]                      NVARCHAR (400) NULL,
    [PageSize]                       INT            NOT NULL,
    [AllowCustomersToSelectPageSize] BIT            NOT NULL,
    [PageSizeOptions]                NVARCHAR (200) NULL,
    [IsMaster]                       BIT            DEFAULT ((0)) NOT NULL,
    [MasterId]                       INT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

