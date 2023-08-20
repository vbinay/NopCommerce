CREATE TABLE [dbo].[PollVotingRecord] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [PollAnswerId] INT      NOT NULL,
    [CustomerId]   INT      NOT NULL,
    [CreatedOnUtc] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [PollVotingRecord_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [PollVotingRecord_PollAnswer] FOREIGN KEY ([PollAnswerId]) REFERENCES [dbo].[PollAnswer] ([Id]) ON DELETE CASCADE
);

