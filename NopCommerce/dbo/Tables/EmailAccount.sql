CREATE TABLE [dbo].[EmailAccount] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [Email]                 NVARCHAR (255) NOT NULL,
    [DisplayName]           NVARCHAR (255) NULL,
    [Host]                  NVARCHAR (255) NOT NULL,
    [Port]                  INT            NOT NULL,
    [Username]              NVARCHAR (255) NOT NULL,
    [Password]              NVARCHAR (255) NOT NULL,
    [EnableSsl]             BIT            NOT NULL,
    [UseDefaultCredentials] BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

