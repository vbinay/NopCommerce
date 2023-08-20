CREATE TABLE [dbo].[RewardPointsHistory] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [CustomerId]       INT             NOT NULL,
    [StoreId]          INT             NOT NULL,
    [Points]           INT             NOT NULL,
    [PointsBalance]    INT             NOT NULL,
    [UsedAmount]       DECIMAL (18, 4) NOT NULL,
    [Message]          NVARCHAR (MAX)  NULL,
    [CreatedOnUtc]     DATETIME        NOT NULL,
    [UsedWithOrder_Id] INT             NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [RewardPointsHistory_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [RewardPointsHistory_UsedWithOrder] FOREIGN KEY ([UsedWithOrder_Id]) REFERENCES [dbo].[Order] ([Id])
);

