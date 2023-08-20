CREATE TABLE [dbo].[Forums_Group] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (200) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    [CreatedOnUtc] DATETIME       NOT NULL,
    [UpdatedOnUtc] DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Forums_Group_DisplayOrder]
    ON [dbo].[Forums_Group]([DisplayOrder] ASC);

