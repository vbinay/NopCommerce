CREATE TABLE [dbo].[CustomerAddresses] (
    [Customer_Id] INT NOT NULL,
    [Address_Id]  INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Customer_Id] ASC, [Address_Id] ASC),
    CONSTRAINT [Customer_Addresses_Source] FOREIGN KEY ([Customer_Id]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [Customer_Addresses_Target] FOREIGN KEY ([Address_Id]) REFERENCES [dbo].[Address] ([Id]) ON DELETE CASCADE
);

