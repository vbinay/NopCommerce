CREATE TABLE [dbo].[StoreMapping] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [EntityId]   INT            NOT NULL,
    [EntityName] NVARCHAR (400) NOT NULL,
    [StoreId]    INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [StoreMapping_Store] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Store] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_StoreMapping_EntityId_EntityName]
    ON [dbo].[StoreMapping]([EntityId] ASC, [EntityName] ASC);

