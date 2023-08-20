CREATE TABLE [dbo].[Forums_Subscription] (
    [Id]               INT              IDENTITY (1, 1) NOT NULL,
    [SubscriptionGuid] UNIQUEIDENTIFIER NOT NULL,
    [CustomerId]       INT              NOT NULL,
    [ForumId]          INT              NOT NULL,
    [TopicId]          INT              NOT NULL,
    [CreatedOnUtc]     DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ForumSubscription_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Subscription_TopicId]
    ON [dbo].[Forums_Subscription]([TopicId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Subscription_ForumId]
    ON [dbo].[Forums_Subscription]([ForumId] ASC);

