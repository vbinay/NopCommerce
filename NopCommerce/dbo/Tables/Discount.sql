CREATE TABLE [dbo].[Discount] (
    [Id]                        INT             IDENTITY (1, 1) NOT NULL,
    [Name]                      NVARCHAR (200)  NOT NULL,
    [DiscountTypeId]            INT             NOT NULL,
    [UsePercentage]             BIT             NOT NULL,
    [DiscountPercentage]        DECIMAL (18, 4) NOT NULL,
    [DiscountAmount]            DECIMAL (18, 4) NOT NULL,
    [MaximumDiscountAmount]     DECIMAL (18, 4) NULL,
    [StartDateUtc]              DATETIME        NULL,
    [EndDateUtc]                DATETIME        NULL,
    [RequiresCouponCode]        BIT             NOT NULL,
    [CouponCode]                NVARCHAR (100)  NULL,
    [IsCumulative]              BIT             NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    [DiscountLimitationId]      INT             NOT NULL,
    [LimitationTimes]           INT             NOT NULL,
    [MaximumDiscountedQuantity] INT             NULL,
    [AppliedToSubCategories]    BIT             NOT NULL,
    [LimitedToStores]           BIT             NULL,	--- SODMYWAY-2987
    [IsMaster]                  BIT             NULL,	--- SODMYWAY-2987
    [MasterId]                  INT             NULL,	--- SODMYWAY-2987
    [Deleted]                   BIT             NULL,	--- SODMYWAY-2987
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);



