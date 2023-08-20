CREATE TABLE [dbo].[ProductReviewHelpfulness] (
    [Id]              INT IDENTITY (1, 1) NOT NULL,
    [ProductReviewId] INT NOT NULL,
    [WasHelpful]      BIT NOT NULL,
    [CustomerId]      INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [ProductReviewHelpfulness_ProductReview] FOREIGN KEY ([ProductReviewId]) REFERENCES [dbo].[ProductReview] ([Id]) ON DELETE CASCADE
);

