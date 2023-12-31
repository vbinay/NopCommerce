﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductDetailsModel : BaseNopEntityModel
    {
        public ProductDetailsModel()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            GiftCard = new GiftCardModel();
            MealPlan = new MealPlanModel();	/// NU-16
            Donation = new DonationModel();	/// NU-17
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<ProductDetailsModel>();
            VendorModel = new VendorBriefInfoModel();
            Breadcrumb = new ProductBreadcrumbModel();
            ProductTags = new List<ProductTagModel>();
            ProductSpecifications= new List<ProductSpecificationModel>();
            ProductManufacturers = new List<ManufacturerModel>();
            ProductReviewOverview = new ProductReviewOverviewModel();
            TierPrices = new List<TierPriceModel>();
        }

        //picture(s)
        public bool DefaultPictureZoomEnabled { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string ProductTemplateViewPath { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public ProductType ProductType { get; set; }

        public bool ShowSku { get; set; }
        public string Sku { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }
        public string Gtin { get; set; }

        public bool ShowVendor { get; set; }
        public VendorBriefInfoModel VendorModel { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModel GiftCard { get; set; }


        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool FreeShippingNotificationEnabled { get; set; }
        public string DeliveryDate { get; set; }


        public bool IsRental { get; set; }
        public DateTime? RentalStartDate { get; set; }
        public DateTime? RentalEndDate { get; set; }

        public string StockAvailability { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }

        public bool EmailAFriendEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }

        public string PageShareCode { get; set; }

        public ProductPriceModel ProductPrice { get; set; }

        public AddToCartModel AddToCart { get; set; }


        public ProductBreadcrumbModel Breadcrumb { get; set; }

        public IList<ProductTagModel> ProductTags { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public IList<ProductSpecificationModel> ProductSpecifications { get; set; }

        public IList<ManufacturerModel> ProductManufacturers { get; set; }

        public ProductReviewOverviewModel ProductReviewOverview { get; set; }

        public IList<TierPriceModel> TierPrices { get; set; }

        //a list of associated products. For example, "Grouped" products could have several child "simple" products
        public IList<ProductDetailsModel> AssociatedProducts { get; set; }

        public bool DisplayDiscontinuedMessage { get; set; }

        public string CurrentStoreName { get; set; }
		
        public MealPlanModel MealPlan { get; set; } /// NU-16

        public DonationModel Donation { get; set; } /// NU-17

        public ReservationProductDetailsModel ReservationProductDetailsModel { get; set; }

        public bool IsReservation { get; set; } //CC-394/400	

        public bool IsBundleProduct { get; set; }
        public string ReservationStartTime { get; set; }
        public string ReservationEndTime { get; set; }

        public int ReservationInterval { get; set; }

        public int MaxOccupancy { get; set; }

        public int ReservationCapPerSlot { get; set; }

        #region Nested Classes

        public partial class ProductBreadcrumbModel : BaseNopModel
        {
            public ProductBreadcrumbModel()
            {
                CategoryBreadcrumb = new List<CategorySimpleModel>();
            }

            public bool Enabled { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public IList<CategorySimpleModel> CategoryBreadcrumb { get; set; }
        }

        public partial class AddToCartModel : BaseNopModel
        {
            public AddToCartModel()
            {
                this.AllowedQuantities = new List<SelectListItem>();
            }
            public int ProductId { get; set; }

            public bool IsReservation { get; set; }

            public bool IsBundleProduct { get; set; }

            //qty
            [NopResourceDisplayName("Products.Qty")]
            public int EnteredQuantity { get; set; }
            public string MinimumQuantityNotification { get; set; }
            //public string ReservationCapPerSlot { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }

            //price entered by customers
            [NopResourceDisplayName("Products.EnterProductPrice")]
            public bool CustomerEntersPrice { get; set; }
            [NopResourceDisplayName("Products.EnterProductPrice")]
            public decimal CustomerEnteredPrice { get; set; }
            public String CustomerEnteredPriceRange { get; set; }

            public bool DisableBuyButton { get; set; }
            //public bool DisableBuyButtonForGuestUsers { get; set; } //NU-90
            public bool DisableWishlistButton { get; set; }

            //rental
            public bool IsRental { get; set; }

            //pre-order
            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            //updating existing shopping cart or wishlist item?
            public int UpdatedShoppingCartItemId { get; set; }
            public ShoppingCartType? UpdateShoppingCartItemType { get; set; }

            public LimitPurchaseResponse LimitPurchaseResult { get; set; }
        }

        public partial class ProductPriceModel : BaseNopModel
        {
            /// <summary>
            /// The currency (in 3-letter ISO 4217 format) of the offer price 
            /// </summary>
            public string CurrencyCode { get; set; }

            public string OldPrice { get; set; }

            public string Price { get; set; }
            public string PriceWithDiscount { get; set; }
            public decimal PriceValue { get; set; }

            public bool CustomerEntersPrice { get; set; }

            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }

            //rental
            public bool IsRental { get; set; }
            public string RentalPrice { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }
        }

        public partial class GiftCardModel : BaseNopModel
        {
            public bool IsGiftCard { get; set; }

            [NopResourceDisplayName("Products.GiftCard.RecipientName")]
            [AllowHtml]
            public string RecipientName { get; set; }
            [NopResourceDisplayName("Products.GiftCard.RecipientEmail")]
            [AllowHtml]
            public string RecipientEmail { get; set; }
            [NopResourceDisplayName("Products.GiftCard.SenderName")]
            [AllowHtml]
            public string SenderName { get; set; }
            [NopResourceDisplayName("Products.GiftCard.SenderEmail")]
            [AllowHtml]
            public string SenderEmail { get; set; }
            [NopResourceDisplayName("Products.GiftCard.Message")]
            [AllowHtml]
            public string Message { get; set; }

            public GiftCardType GiftCardType { get; set; }
        }

	    #region NU-16
        public class MealPlanModel : BaseNopModel
        {
            public bool IsMealPlan { get; set; }

            public bool ShowStandardMealPlanFields { get; set; }

            [NopResourceDisplayName("Products.MealPlan.MealPlanRecipientName")]
            [AllowHtml]
            public string MealPlanRecipientName { get; set; }
            [NopResourceDisplayName("Products.MealPlan.MealPlanRecipientAddress")]
            [AllowHtml]
            public string MealPlanRecipientAddress { get; set; }

            [NopResourceDisplayName("Products.MealPlan.MealPlanRecipientPhone")]
            [AllowHtml]
            public string MealPlanRecipientPhone { get; set; }

            [NopResourceDisplayName("Products.MealPlan.MealPlanRecipientEmail")]
            [AllowHtml]
            public string MealPlanRecipientEmail { get; set; }

            [NopResourceDisplayName("Products.MealPlan.MealPlanRecipientAcctNum")]
            [AllowHtml]
            public string MealPlanRecipientAcctNum { get; set; }
        }
		#endregion
		 
        #region NU-17
        public partial class DonationModel : BaseNopModel
        {
            public bool IsDonation { get; set; }

            public bool ShowOnBehalfOf { get; set; }


            [NopResourceDisplayName("Products.Donation.DonorFirstName")]
            [AllowHtml]
            public string DonorFirstName { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorLastName")]
            [AllowHtml]
            public string DonorLastName { get; set; }


            [NopResourceDisplayName("Products.Donation.DonorCompany")]
            [AllowHtml]
            public string DonorCompany { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorAddress")]
            [AllowHtml]
            public string DonorAddress { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorAddress2")]
            [AllowHtml]
            public string DonorAddress2 { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorCity")]
            [AllowHtml]
            public string DonorCity { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorState")]
            [AllowHtml]
            public string DonorState { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorZip")]
            [AllowHtml]
            public string DonorZip { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorPhone")]
            [AllowHtml]
            public string DonorPhone { get; set; }

            [NopResourceDisplayName("Products.Donation.DonorCountry")]
            [AllowHtml]
            public string DonorCountry { get; set; }

            [NopResourceDisplayName("Products.Donation.IsOnBehalf")]
            [AllowHtml]
            public bool IsOnBehalf { get; set; }

            [NopResourceDisplayName("Products.Donation.OnBehalfOfFullName")]
            [AllowHtml]
            public string OnBehalfOfFullName { get; set; }

            [NopResourceDisplayName("Products.Donation.NotificationEmail")]
            [AllowHtml]
            public string NotificationEmail { get; set; }

            [NopResourceDisplayName("Products.Donation.IncludeGiftAmount")]
            [AllowHtml]
            public bool IncludeGiftAmount { get; set; }

            [NopResourceDisplayName("Products.Donation.Comments")]
            [AllowHtml]
            public string Comments { get; set; }
        }
        #endregion

        public partial class TierPriceModel : BaseNopModel
        {
            public string Price { get; set; }

            public int Quantity { get; set; }
        }

        public partial class ProductAttributeModel : BaseNopEntityModel
        {
            public ProductAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<ProductAttributeValueModel>();
            }

            public int ProductId { get; set; }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }
            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// A value indicating whether this attribute depends on some other attribute
            /// </summary>
            public bool HasCondition { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ProductAttributeValueModel> Values { get; set; }

        }

        public partial class ProductAttributeValueModel : BaseNopEntityModel
        {
            public ProductAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            //picture model is used with "image square" attribute type
            public PictureModel ImageSquaresPictureModel { get; set; }

            public string PriceAdjustment { get; set; }

            public decimal PriceAdjustmentValue { get; set; }

            public bool IsPreSelected { get; set; }

            //product picture ID (associated to this value)
            public int PictureId { get; set; }
        }

		#endregion
    }
}