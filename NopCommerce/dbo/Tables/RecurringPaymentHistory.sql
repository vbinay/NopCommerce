CREATE TABLE [dbo].[RecurringPaymentHistory] (
    [Id]                 INT      IDENTITY (1, 1) NOT NULL,
    [RecurringPaymentId] INT      NOT NULL,
    [OrderId]            INT      NOT NULL,
    [CreatedOnUtc]       DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [RecurringPaymentHistory_RecurringPayment] FOREIGN KEY ([RecurringPaymentId]) REFERENCES [dbo].[RecurringPayment] ([Id]) ON DELETE CASCADE
);

