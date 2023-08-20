CREATE TABLE [dbo].[ReturnRequest] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [StoreId]               INT            NOT NULL,
    [OrderItemId]           INT            NOT NULL,
    [CustomerId]            INT            NOT NULL,
    [Quantity]              INT            NOT NULL,
    [ReasonForReturn]       NVARCHAR (MAX) NOT NULL,
    [RequestedAction]       NVARCHAR (MAX) NOT NULL,
    [CustomerComments]      NVARCHAR (MAX) NULL,
    [StaffNotes]            NVARCHAR (MAX) NULL,
    [ReturnRequestStatusId] INT            NOT NULL,
    [CreatedOnUtc]          DATETIME       NOT NULL,
    [UpdatedOnUtc]          DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ReturnRequest_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE
);

