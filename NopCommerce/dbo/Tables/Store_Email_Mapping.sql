--- #region SODMYWAY-2957
CREATE TABLE [dbo].[Store_Email_Mapping] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [EmailTypeId] INT            NOT NULL,
    [Email]       NVARCHAR (255) NOT NULL,
    [DisplayName] NVARCHAR (255) NULL,
    [IsMaster]    BIT            NULL,
    [MasterId]    INT            NULL,
    [StoreId]     INT            NULL,
    [Deleted]     BIT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_EmailContact_Store] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Store] ([Id]),
    CONSTRAINT [FK_EmailContacts_EmailType] FOREIGN KEY ([EmailTypeId]) REFERENCES [dbo].[EmailType] ([Id])
);

--- #endregion