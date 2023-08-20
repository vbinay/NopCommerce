CREATE TABLE [dbo].[DiscountRequirement] (
    [Id]                                INT            IDENTITY (1, 1) NOT NULL,
    [DiscountId]                        INT            NOT NULL,
    [DiscountRequirementRuleSystemName] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Discount_DiscountRequirements] FOREIGN KEY ([DiscountId]) REFERENCES [dbo].[Discount] ([Id]) ON DELETE CASCADE
);

