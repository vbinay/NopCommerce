CREATE TABLE [dbo].[ProductTag] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (400) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ProductTag_Name]
    ON [dbo].[ProductTag]([Name] ASC);

