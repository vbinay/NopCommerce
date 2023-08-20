CREATE TABLE [dbo].[TopicTemplate] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (400) NOT NULL,
    [ViewPath]     NVARCHAR (400) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

