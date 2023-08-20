CREATE TABLE [dbo].[Forums_PrivateMessage] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [StoreId]              INT            NOT NULL,
    [FromCustomerId]       INT            NOT NULL,
    [ToCustomerId]         INT            NOT NULL,
    [Subject]              NVARCHAR (450) NOT NULL,
    [Text]                 NVARCHAR (MAX) NOT NULL,
    [IsRead]               BIT            NOT NULL,
    [IsDeletedByAuthor]    BIT            NOT NULL,
    [IsDeletedByRecipient] BIT            NOT NULL,
    [CreatedOnUtc]         DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [PrivateMessage_FromCustomer] FOREIGN KEY ([FromCustomerId]) REFERENCES [dbo].[Customer] ([Id]),
    CONSTRAINT [PrivateMessage_ToCustomer] FOREIGN KEY ([ToCustomerId]) REFERENCES [dbo].[Customer] ([Id])
);

