CREATE TABLE [dbo].[RecurringPayment] (
    [Id]             INT      IDENTITY (1, 1) NOT NULL,
    [CycleLength]    INT      NOT NULL,
    [CyclePeriodId]  INT      NOT NULL,
    [TotalCycles]    INT      NOT NULL,
    [StartDateUtc]   DATETIME NOT NULL,
    [IsActive]       BIT      NOT NULL,
    [Deleted]        BIT      NOT NULL,
    [InitialOrderId] INT      NOT NULL,
    [CreatedOnUtc]   DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [RecurringPayment_InitialOrder] FOREIGN KEY ([InitialOrderId]) REFERENCES [dbo].[Order] ([Id])
);

