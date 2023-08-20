CREATE TABLE [dbo].[AclRecord] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [EntityId]       INT            NOT NULL,
    [EntityName]     NVARCHAR (400) NOT NULL,
    [CustomerRoleId] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [AclRecord_CustomerRole] FOREIGN KEY ([CustomerRoleId]) REFERENCES [dbo].[CustomerRole] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AclRecord_EntityId_EntityName]
    ON [dbo].[AclRecord]([EntityId] ASC, [EntityName] ASC);

