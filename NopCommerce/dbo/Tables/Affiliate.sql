CREATE TABLE [dbo].[Affiliate] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [AddressId]       INT            NOT NULL,
    [AdminComment]    NVARCHAR (MAX) NULL,
    [FriendlyUrlName] NVARCHAR (MAX) NULL,
    [Deleted]         BIT            NOT NULL,
    [Active]          BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Affiliate_Address] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id])
);

