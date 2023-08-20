CREATE TABLE [dbo].[Download] (
    [Id]             INT              IDENTITY (1, 1) NOT NULL,
    [DownloadGuid]   UNIQUEIDENTIFIER NOT NULL,
    [UseDownloadUrl] BIT              NOT NULL,
    [DownloadUrl]    NVARCHAR (MAX)   NULL,
    [DownloadBinary] VARBINARY (MAX)  NULL,
    [ContentType]    NVARCHAR (MAX)   NULL,
    [Filename]       NVARCHAR (MAX)   NULL,
    [Extension]      NVARCHAR (MAX)   NULL,
    [IsNew]          BIT              NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

