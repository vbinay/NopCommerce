CREATE TABLE [dbo].[Customer_CustomerRole_Mapping] (
    [Customer_Id]     INT NOT NULL,
    [CustomerRole_Id] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Customer_Id] ASC, [CustomerRole_Id] ASC),
    CONSTRAINT [Customer_CustomerRoles_Source] FOREIGN KEY ([Customer_Id]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Customer_CustomerRoles_Target] FOREIGN KEY ([CustomerRole_Id]) REFERENCES [dbo].[CustomerRole] ([Id]) ON DELETE CASCADE
);

