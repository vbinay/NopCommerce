CREATE TABLE [dbo].[Store] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [Name]               NVARCHAR (400)  NOT NULL,
    [Url]                NVARCHAR (400)  NOT NULL,
    [SslEnabled]         BIT             NOT NULL,
    [SecureUrl]          NVARCHAR (400)  NULL,
    [Hosts]              NVARCHAR (1000) NULL,
    [DefaultLanguageId]  INT             NOT NULL,
    [DisplayOrder]       INT             NOT NULL,
    [CompanyName]        NVARCHAR (1000) NULL,
    [CompanyAddress]     NVARCHAR (1000) NULL,
    [CompanyPhoneNumber] NVARCHAR (1000) NULL,
    [CompanyVat]         NVARCHAR (1000) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

