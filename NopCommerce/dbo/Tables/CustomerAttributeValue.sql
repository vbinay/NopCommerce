CREATE TABLE [dbo].[CustomerAttributeValue] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [CustomerAttributeId] INT            NOT NULL,
    [Name]                NVARCHAR (400) NOT NULL,
    [IsPreSelected]       BIT            NOT NULL,
    [DisplayOrder]        INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CustomerAttributeValue_CustomerAttribute] FOREIGN KEY ([CustomerAttributeId]) REFERENCES [dbo].[CustomerAttribute] ([Id]) ON DELETE CASCADE
);

