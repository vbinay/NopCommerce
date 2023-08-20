CREATE TABLE [dbo].[GenericAttribute] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [EntityId] INT            NOT NULL,
    [KeyGroup] NVARCHAR (400) NOT NULL,
    [Key]      NVARCHAR (400) NOT NULL,
    [Value]    NVARCHAR (MAX) NOT NULL,
    [StoreId]  INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_GenericAttribute_EntityId_and_KeyGroup]
    ON [dbo].[GenericAttribute]([EntityId] ASC, [KeyGroup] ASC);

