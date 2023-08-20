CREATE TABLE [dbo].[NewsLetterSubscription] (
    [Id]                         INT              IDENTITY (1, 1) NOT NULL,
    [NewsLetterSubscriptionGuid] UNIQUEIDENTIFIER NOT NULL,
    [Email]                      NVARCHAR (255)   NOT NULL,
    [Active]                     BIT              NOT NULL,
    [StoreId]                    INT              NOT NULL,
    [CreatedOnUtc]               DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_NewsletterSubscription_Email_StoreId]
    ON [dbo].[NewsLetterSubscription]([Email] ASC, [StoreId] ASC);

