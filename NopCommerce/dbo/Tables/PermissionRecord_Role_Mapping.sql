CREATE TABLE [dbo].[PermissionRecord_Role_Mapping] (
    [PermissionRecord_Id] INT NOT NULL,
    [CustomerRole_Id]     INT NOT NULL,
    PRIMARY KEY CLUSTERED ([PermissionRecord_Id] ASC, [CustomerRole_Id] ASC),
    CONSTRAINT [PermissionRecord_CustomerRoles_Source] FOREIGN KEY ([PermissionRecord_Id]) REFERENCES [dbo].[PermissionRecord] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [PermissionRecord_CustomerRoles_Target] FOREIGN KEY ([CustomerRole_Id]) REFERENCES [dbo].[CustomerRole] ([Id]) ON DELETE CASCADE
);

