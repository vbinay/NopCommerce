CREATE TABLE [dbo].[AddressAttributeValue] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [AddressAttributeId] INT            NOT NULL,
    [Name]               NVARCHAR (400) NOT NULL,
    [IsPreSelected]      BIT            NOT NULL,
    [DisplayOrder]       INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [AddressAttributeValue_AddressAttribute] FOREIGN KEY ([AddressAttributeId]) REFERENCES [dbo].[AddressAttribute] ([Id]) ON DELETE CASCADE
);

