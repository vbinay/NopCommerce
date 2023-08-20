CREATE TABLE [dbo].[CheckoutAttributeValue] (
    [Id]                  INT             IDENTITY (1, 1) NOT NULL,
    [CheckoutAttributeId] INT             NOT NULL,
    [Name]                NVARCHAR (400)  NOT NULL,
    [ColorSquaresRgb]     NVARCHAR (100)  NULL,
    [PriceAdjustment]     DECIMAL (18, 4) NOT NULL,
    [WeightAdjustment]    DECIMAL (18, 4) NOT NULL,
    [IsPreSelected]       BIT             NOT NULL,
    [DisplayOrder]        INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CheckoutAttributeValue_CheckoutAttribute] FOREIGN KEY ([CheckoutAttributeId]) REFERENCES [dbo].[CheckoutAttribute] ([Id]) ON DELETE CASCADE
);

