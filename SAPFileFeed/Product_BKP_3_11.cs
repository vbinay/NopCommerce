//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SAPFileFeed
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product_BKP_3_11
    {
        public int Id { get; set; }
        public int ProductTypeId { get; set; }
        public int ParentGroupedProductId { get; set; }
        public bool VisibleIndividually { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdminComment { get; set; }
        public int ProductTemplateId { get; set; }
        public int VendorId { get; set; }
        public bool ShowOnHomePage { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int ApprovedRatingSum { get; set; }
        public int NotApprovedRatingSum { get; set; }
        public int ApprovedTotalReviews { get; set; }
        public int NotApprovedTotalReviews { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Gtin { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public Nullable<decimal> OverriddenGiftCardAmount { get; set; }
        public bool RequireOtherProducts { get; set; }
        public string RequiredProductIds { get; set; }
        public bool AutomaticallyAddRequiredProducts { get; set; }
        public bool IsDownload { get; set; }
        public int DownloadId { get; set; }
        public bool UnlimitedDownloads { get; set; }
        public int MaxNumberOfDownloads { get; set; }
        public Nullable<int> DownloadExpirationDays { get; set; }
        public int DownloadActivationTypeId { get; set; }
        public bool HasSampleDownload { get; set; }
        public int SampleDownloadId { get; set; }
        public bool HasUserAgreement { get; set; }
        public string UserAgreementText { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurringCycleLength { get; set; }
        public int RecurringCyclePeriodId { get; set; }
        public int RecurringTotalCycles { get; set; }
        public bool IsRental { get; set; }
        public int RentalPriceLength { get; set; }
        public int RentalPricePeriodId { get; set; }
        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool ShipSeparately { get; set; }
        public decimal AdditionalShippingCharge { get; set; }
        public int DeliveryDateId { get; set; }
        public bool IsTaxExempt { get; set; }
        public int TaxCategoryId { get; set; }
        public bool IsTelecommunicationsOrBroadcastingOrElectronicServices { get; set; }
        public int ManageInventoryMethodId { get; set; }
        public bool UseMultipleWarehouses { get; set; }
        public int WarehouseId { get; set; }
        public int StockQuantity { get; set; }
        public bool DisplayStockAvailability { get; set; }
        public bool DisplayStockQuantity { get; set; }
        public int MinStockQuantity { get; set; }
        public int LowStockActivityId { get; set; }
        public int NotifyAdminForQuantityBelow { get; set; }
        public int BackorderModeId { get; set; }
        public bool AllowBackInStockSubscriptions { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public string AllowedQuantities { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public bool NotReturnable { get; set; }
        public bool DisableBuyButton { get; set; }
        public bool DisableWishlistButton { get; set; }
        public bool AvailableForPreOrder { get; set; }
        public Nullable<System.DateTime> PreOrderAvailabilityStartDateTimeUtc { get; set; }
        public bool CallForPrice { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal ProductCost { get; set; }
        public Nullable<decimal> SpecialPrice { get; set; }
        public Nullable<System.DateTime> SpecialPriceStartDateTimeUtc { get; set; }
        public Nullable<System.DateTime> SpecialPriceEndDateTimeUtc { get; set; }
        public bool CustomerEntersPrice { get; set; }
        public decimal MinimumCustomerEnteredPrice { get; set; }
        public decimal MaximumCustomerEnteredPrice { get; set; }
        public bool BasepriceEnabled { get; set; }
        public decimal BasepriceAmount { get; set; }
        public int BasepriceUnitId { get; set; }
        public decimal BasepriceBaseAmount { get; set; }
        public int BasepriceBaseUnitId { get; set; }
        public bool MarkAsNew { get; set; }
        public Nullable<System.DateTime> MarkAsNewStartDateTimeUtc { get; set; }
        public Nullable<System.DateTime> MarkAsNewEndDateTimeUtc { get; set; }
        public bool HasTierPrices { get; set; }
        public bool HasDiscountsApplied { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public Nullable<System.DateTime> AvailableStartDateTimeUtc { get; set; }
        public Nullable<System.DateTime> AvailableEndDateTimeUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public System.DateTime UpdatedOnUtc { get; set; }
        public bool IsMaster { get; set; }
        public int MasterId { get; set; }
        public bool IsMealPlan { get; set; }
        public bool IsDonation { get; set; }
        public Nullable<int> SiteProductId { get; set; }
        public Nullable<int> ProductionDaysLead { get; set; }
        public Nullable<int> ProductionHoursLead { get; set; }
        public Nullable<int> ProductionMinutesLead { get; set; }
        public bool IsDeliveryEnabled { get; set; }
        public bool IsPickupEnabled { get; set; }
        public string CheckoutNotes { get; set; }
        public string URL { get; set; }
        public string CardAccessProgramCode { get; set; }
        public Nullable<int> PreferredPickupWarehouseId { get; set; }
        public Nullable<decimal> StoreCommission { get; set; }
        public Nullable<bool> ShowStandardMealPlanFields { get; set; }
        public Nullable<bool> DisableBuyButtonForGuestUsers { get; set; }
        public int CustomerMaximumQuantity { get; set; }
        public bool LimitPurchase { get; set; }
        public Nullable<int> LimitPurchaseType { get; set; }
        public string AlternateProductName { get; set; }
        public Nullable<bool> IsLocalDelivery { get; set; }
        public Nullable<bool> IsCorporateDonation { get; set; }
    }
}
