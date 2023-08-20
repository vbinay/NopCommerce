CREATE TABLE [dbo].[Product] (
    [Id]                                                     INT             IDENTITY (1, 1) NOT NULL,
    [ProductTypeId]                                          INT             NOT NULL,
    [ParentGroupedProductId]                                 INT             NOT NULL,
    [VisibleIndividually]                                    BIT             NOT NULL,
    [Name]                                                   NVARCHAR (400)  NOT NULL,
    [ShortDescription]                                       NVARCHAR (MAX)  NULL,
    [FullDescription]                                        NVARCHAR (MAX)  NULL,
    [AdminComment]                                           NVARCHAR (MAX)  NULL,
    [ProductTemplateId]                                      INT             NOT NULL,
    [VendorId]                                               INT             NOT NULL,
    [ShowOnHomePage]                                         BIT             NOT NULL,
    [MetaKeywords]                                           NVARCHAR (400)  NULL,
    [MetaDescription]                                        NVARCHAR (MAX)  NULL,
    [MetaTitle]                                              NVARCHAR (400)  NULL,
    [AllowCustomerReviews]                                   BIT             NOT NULL,
    [ApprovedRatingSum]                                      INT             NOT NULL,
    [NotApprovedRatingSum]                                   INT             NOT NULL,
    [ApprovedTotalReviews]                                   INT             NOT NULL,
    [NotApprovedTotalReviews]                                INT             NOT NULL,
    [SubjectToAcl]                                           BIT             NOT NULL,
    [LimitedToStores]                                        BIT             NOT NULL,
    [Sku]                                                    NVARCHAR (400)  NULL,
    [ManufacturerPartNumber]                                 NVARCHAR (400)  NULL,
    [Gtin]                                                   NVARCHAR (400)  NULL,
    [IsGiftCard]                                             BIT             NOT NULL,
    [GiftCardTypeId]                                         INT             NOT NULL,
    [OverriddenGiftCardAmount]                               DECIMAL (18, 2) NULL,
    [RequireOtherProducts]                                   BIT             NOT NULL,
    [RequiredProductIds]                                     NVARCHAR (1000) NULL,
    [AutomaticallyAddRequiredProducts]                       BIT             NOT NULL,
    [IsDownload]                                             BIT             NOT NULL,
    [DownloadId]                                             INT             NOT NULL,
    [UnlimitedDownloads]                                     BIT             NOT NULL,
    [MaxNumberOfDownloads]                                   INT             NOT NULL,
    [DownloadExpirationDays]                                 INT             NULL,
    [DownloadActivationTypeId]                               INT             NOT NULL,
    [HasSampleDownload]                                      BIT             NOT NULL,
    [SampleDownloadId]                                       INT             NOT NULL,
    [HasUserAgreement]                                       BIT             NOT NULL,
    [UserAgreementText]                                      NVARCHAR (MAX)  NULL,
    [IsRecurring]                                            BIT             NOT NULL,
    [RecurringCycleLength]                                   INT             NOT NULL,
    [RecurringCyclePeriodId]                                 INT             NOT NULL,
    [RecurringTotalCycles]                                   INT             NOT NULL,
    [IsRental]                                               BIT             NOT NULL,
    [RentalPriceLength]                                      INT             NOT NULL,
    [RentalPricePeriodId]                                    INT             NOT NULL,
    [IsShipEnabled]                                          BIT             NOT NULL,
    [IsFreeShipping]                                         BIT             NOT NULL,
    [ShipSeparately]                                         BIT             NOT NULL,
    [AdditionalShippingCharge]                               DECIMAL (18, 4) NOT NULL,
    [DeliveryDateId]                                         INT             NOT NULL,
    [IsTaxExempt]                                            BIT             NOT NULL,
    [TaxCategoryId]                                          INT             NOT NULL,
    [IsTelecommunicationsOrBroadcastingOrElectronicServices] BIT             NOT NULL,
    [ManageInventoryMethodId]                                INT             NOT NULL,
    [UseMultipleWarehouses]                                  BIT             NOT NULL,
    [WarehouseId]                                            INT             NOT NULL,
    [StockQuantity]                                          INT             NOT NULL,
    [DisplayStockAvailability]                               BIT             NOT NULL,
    [DisplayStockQuantity]                                   BIT             NOT NULL,
    [MinStockQuantity]                                       INT             NOT NULL,
    [LowStockActivityId]                                     INT             NOT NULL,
    [NotifyAdminForQuantityBelow]                            INT             NOT NULL,
    [BackorderModeId]                                        INT             NOT NULL,
    [AllowBackInStockSubscriptions]                          BIT             NOT NULL,
    [OrderMinimumQuantity]                                   INT             NOT NULL,
    [OrderMaximumQuantity]                                   INT             NOT NULL,
    [AllowedQuantities]                                      NVARCHAR (1000) NULL,
    [AllowAddingOnlyExistingAttributeCombinations]           BIT             NOT NULL,
    [NotReturnable]                                          BIT             NOT NULL DEFAULT 0,	--- SODMYWAY-3297
    [DisableBuyButton]                                       BIT             NOT NULL ,
    [DisableWishlistButton]                                  BIT             NOT NULL ,
    [AvailableForPreOrder]                                   BIT             NOT NULL,
    [PreOrderAvailabilityStartDateTimeUtc]                   DATETIME        NULL,
    [CallForPrice]                                           BIT             NOT NULL,
    [Price]                                                  DECIMAL (18, 4) NOT NULL,
    [OldPrice]                                               DECIMAL (18, 4) NOT NULL,
    [ProductCost]                                            DECIMAL (18, 4) NOT NULL,
    [SpecialPrice]                                           DECIMAL (18, 4) NULL,
    [SpecialPriceStartDateTimeUtc]                           DATETIME        NULL,
    [SpecialPriceEndDateTimeUtc]                             DATETIME        NULL,
    [CustomerEntersPrice]                                    BIT             NOT NULL,
    [MinimumCustomerEnteredPrice]                            DECIMAL (18, 4) NOT NULL,
    [MaximumCustomerEnteredPrice]                            DECIMAL (18, 4) NOT NULL,
    [BasepriceEnabled]                                       BIT             NOT NULL,
    [BasepriceAmount]                                        DECIMAL (18, 4) NOT NULL,
    [BasepriceUnitId]                                        INT             NOT NULL,
    [BasepriceBaseAmount]                                    DECIMAL (18, 4) NOT NULL,
    [BasepriceBaseUnitId]                                    INT             NOT NULL,
    [MarkAsNew]                                              BIT             NOT NULL,
    [MarkAsNewStartDateTimeUtc]                              DATETIME        NULL,
    [MarkAsNewEndDateTimeUtc]                                DATETIME        NULL,
    [HasTierPrices]                                          BIT             NOT NULL,
    [HasDiscountsApplied]                                    BIT             NOT NULL,
    [Weight]                                                 DECIMAL (18, 4) NOT NULL,
    [Length]                                                 DECIMAL (18, 4) NOT NULL,
    [Width]                                                  DECIMAL (18, 4) NOT NULL,
    [Height]                                                 DECIMAL (18, 4) NOT NULL,
    [AvailableStartDateTimeUtc]                              DATETIME        NULL,
    [AvailableEndDateTimeUtc]                                DATETIME        NULL,
    [DisplayOrder]                                           INT             NOT NULL,
    [Published]                                              BIT             NOT NULL,
    [Deleted]                                                BIT             NOT NULL,
    [CreatedOnUtc]                                           DATETIME        NOT NULL,
    [UpdatedOnUtc]                                           DATETIME        NOT NULL,
    [IsMaster]                                               BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [MasterId]                                               INT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [IsMealPlan]                                             BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [IsDonation]                                             BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [SiteProductId]                                          INT             NULL,	--- SODMYWAY-2957
    [ProductionDaysLead]                                     INT             DEFAULT ((0)) NULL,	--- SODMYWAY-2957
    [ProductionHoursLead]                                    INT             DEFAULT ((0)) NULL,	--- SODMYWAY-2957
    [ProductionMinutesLead]                                  INT             DEFAULT ((0)) NULL,	--- SODMYWAY-2957
    [IsDeliveryEnabled]                                      BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [IsPickupEnabled]                                        BIT             DEFAULT ((0)) NOT NULL,	--- SODMYWAY-2957
    [CheckoutNotes]                                          NVARCHAR (MAX)  NULL,	--- SODMYWAY-2957
    [URL]                                                    NVARCHAR (2048) NULL,	--- SODMYWAY-2957
    [CardAccessProgramCode]                                  NVARCHAR (MAX)  NULL,	--- SODMYWAY-2957
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);






GO
CREATE NONCLUSTERED INDEX [IX_Product_SubjectToAcl]
    ON [dbo].[Product]([SubjectToAcl] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_LimitedToStores]
    ON [dbo].[Product]([LimitedToStores] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_VisibleIndividually]
    ON [dbo].[Product]([VisibleIndividually] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_ParentGroupedProductId]
    ON [dbo].[Product]([ParentGroupedProductId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_ShowOnHomepage]
    ON [dbo].[Product]([ShowOnHomePage] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_Published]
    ON [dbo].[Product]([Published] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_Deleted_and_Published]
    ON [dbo].[Product]([Published] ASC, [Deleted] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_PriceDatesEtc]
    ON [dbo].[Product]([Price] ASC, [AvailableStartDateTimeUtc] ASC, [AvailableEndDateTimeUtc] ASC, [Published] ASC, [Deleted] ASC);

