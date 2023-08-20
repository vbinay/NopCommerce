CREATE TABLE [dbo].[Poll] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [LanguageId]        INT            NOT NULL,
    [Name]              NVARCHAR (MAX) NOT NULL,
    [SystemKeyword]     NVARCHAR (MAX) NULL,
    [Published]         BIT            NOT NULL,
    [ShowOnHomePage]    BIT            NOT NULL,
    [AllowGuestsToVote] BIT            NOT NULL,
    [DisplayOrder]      INT            NOT NULL,
    [StartDateUtc]      DATETIME       NULL,
    [EndDateUtc]        DATETIME       NULL,
    [LimitedToStores]   BIT            CONSTRAINT [DF_Poll_LimitedToStores] DEFAULT ((0)) NULL,	--- SODMYWAY-2946
    [IsMaster]          BIT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
    [MasterId]          INT            DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2945
	PRIMARY KEY CLUSTERED ([Id] ASC),	--- SODMYWAY-3297
    CONSTRAINT [Poll_Language] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]) ON DELETE CASCADE	--- SODMYWAY-3297
);

