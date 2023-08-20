CREATE TABLE [dbo].[PollAnswer] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [PollId]        INT            NOT NULL,
    [Name]          NVARCHAR (MAX) NOT NULL,
    [NumberOfVotes] INT            NOT NULL,
    [DisplayOrder]  INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [PollAnswer_Poll] FOREIGN KEY ([PollId]) REFERENCES [dbo].[Poll] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PollAnswer_PollId]
    ON [dbo].[PollAnswer]([PollId] ASC);

