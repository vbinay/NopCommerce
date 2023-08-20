CREATE TABLE [dbo].[Language] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (100) NOT NULL,
    [LanguageCulture]   NVARCHAR (20)  NOT NULL,
    [UniqueSeoCode]     NVARCHAR (2)   NULL,
    [FlagImageFileName] NVARCHAR (50)  NULL,
    [Rtl]               BIT            NOT NULL,
    [LimitedToStores]   BIT            NOT NULL,
    [DefaultCurrencyId] INT            NOT NULL,
    [Published]         BIT            NOT NULL,
    [DisplayOrder]      INT            NOT NULL,
    [IsMaster]          BIT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    [MasterId]          INT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Language_DisplayOrder]
    ON [dbo].[Language]([DisplayOrder] ASC);

