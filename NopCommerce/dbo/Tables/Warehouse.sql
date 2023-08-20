CREATE TABLE [dbo].[Warehouse] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (400)  NOT NULL,
    [AdminComment]      NVARCHAR (MAX)  NULL,
    [AddressId]         INT             NOT NULL,
    [IsMaster]          BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [MasterId]          INT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [LimitedToStores]   BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [Email]             NVARCHAR (4000) NULL,	--- SODMYWAY-2944
    [LeadTimeDays]      INT             NULL,	--- SODMYWAY-2944
    [LeadTimeHours]     INT             NULL,	--- SODMYWAY-2944
    [LeadTimeMinutes]   INT             NULL,	--- SODMYWAY-2944
    [OpenTime]          NVARCHAR (255)  NULL,	--- SODMYWAY-2944
    [CloseTime]         NVARCHAR (255)  NULL,	--- SODMYWAY-2944
    [DeliveryCloseTime] NVARCHAR (255)  NULL,	--- SODMYWAY-2941
    [DeliveryOpenTime]  NVARCHAR (255)  NULL,	--- SODMYWAY-2941
    [PickupCloseTime]   NVARCHAR (255)  NULL,	--- SODMYWAY-2941
    [PickupOpenTime]    NVARCHAR (255)  NULL,	--- SODMYWAY-2941
    [IsDelivery]        BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2941
    [IsPickup]          BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2941
    [AllowPickupTime]   BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2941
    [AllowDeliveryTime] BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2941
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);



