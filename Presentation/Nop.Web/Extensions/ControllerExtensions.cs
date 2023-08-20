using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Common;

namespace Nop.Web.Extensions
{
    //here we have some methods shared between controllers
    public static class ControllerExtensions
    {
        public static IList<ProductSpecificationModel> PrepareProductSpecificationModel(this Controller controller,
            IWorkContext workContext,
            ISpecificationAttributeService specificationAttributeService,
            ICacheManager cacheManager,
            Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, workContext.WorkingLanguage.Id);
            return cacheManager.Get(cacheKey, () =>
                specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
                .Select(psa =>
                {
                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
                        ColorSquaresRgb = psa.SpecificationAttributeOption.ColorSquaresRgb
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = HttpUtility.HtmlEncode(psa.SpecificationAttributeOption.GetLocalized(x => x.Name));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = HttpUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = psa.CustomValue;
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = string.Format("<a href='{0}' target='_blank'>{0}</a>", psa.CustomValue);
                            break;
                        default:
                            break;
                    }
                    return m;
                }).ToList()
            );
        }

        public static IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(this Controller controller,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICategoryService categoryService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPictureService pictureService,
            IMeasureService measureService,
            IOrderService orderService,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings,
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
                    ProductTemplateId = product.ProductTemplateId,	/// SODMYWAY-
                    Url = product.Url,	/// SODMYWAY-

                };

                #region SODMYWAY-
                var allowedQuantities = product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    model.AllowedQuantities.Add(new SelectListItem
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = false
                    });
                }
                #endregion
                //price



                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel
                    {
                        ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
                    };


                    switch (product.ProductType)
                    {
                        case ProductType.GroupedProduct:
                            {
                                #region Grouped product

                                var associatedProducts = productService.GetAssociatedProducts(product.Id, storeContext.CurrentStore.Id);

                                //add to cart button (ignore "DisableBuyButton" property for grouped products)
                                priceModel.DisableBuyButton = !permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                                    !permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                                priceModel.DisableWishlistButton = !permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                    !permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //compare products
                                priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;
                                switch (associatedProducts.Count)
                                {
                                    case 0:
                                        {
                                            //no associated products
                                        }
                                        break;
                                    default:
                                        {
                                            //we have at least one associated product
                                            //compare products
                                            priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;
                                            //priceModel.AvailableForPreOrder = false;

                                            if (permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                            {
                                                //find a minimum possible price
                                                decimal? minPossiblePrice = null;
                                                Product minPriceProduct = null;
                                                foreach (var associatedProduct in associatedProducts)
                                                {
                                                    //calculate for the maximum quantity (in case if we have tier prices)
                                                    var tmpPrice = priceCalculationService.GetFinalPrice(associatedProduct,
                                                        workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);
                                                    if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                                    {
                                                        minPriceProduct = associatedProduct;
                                                        minPossiblePrice = tmpPrice;
                                                    }
                                                }
                                                if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                                {
                                                    if (minPriceProduct.CallForPrice)
                                                    {
                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = localizationService.GetResource("Products.CallForPrice");
                                                    }
                                                    else if (minPossiblePrice.HasValue)
                                                    {
                                                        //calculate prices
                                                        decimal taxRate;
                                                        decimal finalPriceBase = taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                        decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workContext.WorkingCurrency);

                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = String.Format(localizationService.GetResource("Products.PriceRangeFrom"), priceFormatter.FormatPrice(finalPrice));
                                                        priceModel.PriceValue = finalPrice;

                                                        //PAngV baseprice (used in Germany)
                                                        priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
                                                            localizationService, measureService, currencyService, workContext, priceFormatter);
                                                    }
                                                    else
                                                    {
                                                        //Actually it's not possible (we presume that minimalPrice always has a value)
                                                        //We never should get here
                                                        Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //hide prices
                                                priceModel.OldPrice = null;
                                                priceModel.Price = null;
                                            }
                                        }
                                        break;
                                }

                                #endregion
                            }
                            break;
                        case ProductType.SimpleProduct:
                        default:
                            {
                                #region Simple product

                                //add to cart button
                                priceModel.DisableBuyButton = product.DisableBuyButton ||
                                    !permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                                    !permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //add to wishlist button
                                priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                    !permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                    !permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
                                //compare products
                                priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;

                                //rental
                                priceModel.IsRental = product.IsRental;

                                //pre-order
                                if (product.AvailableForPreOrder)
                                {
                                    priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                        product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                                    priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                                }

                                //prices
                                if (permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                {
                                    if (!product.CustomerEntersPrice)
                                    {
                                        if (product.CallForPrice)
                                        {
                                            //call for price
                                            priceModel.OldPrice = null;
                                            priceModel.Price = localizationService.GetResource("Products.CallForPrice");
                                        }
                                        else
                                        {
                                            //prices

                                            //calculate for the maximum quantity (in case if we have tier prices)
                                            decimal minPossiblePrice = priceCalculationService.GetFinalPrice(product,
                                                workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

                                            decimal taxRate;
                                            decimal oldPriceBase = taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                            decimal finalPriceBase = taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                            decimal oldPrice = currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, workContext.WorkingCurrency);
                                            decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workContext.WorkingCurrency);

                                            //do we have tier prices configured?
                                            var tierPrices = new List<TierPrice>();
                                            if (product.HasTierPrices)
                                            {
                                                tierPrices.AddRange(product.TierPrices
                                                    .OrderBy(tp => tp.Quantity)
                                                    .ToList()
                                                    .FilterByStore(storeContext.CurrentStore.Id)
                                                    .FilterForCustomer(workContext.CurrentCustomer)
                                                    .RemoveDuplicatedQuantities());
                                            }
                                            //When there is just one tier (with  qty 1), 
                                            //there are no actual savings in the list.
                                            bool displayFromMessage = tierPrices.Any() &&
                                                !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                            if (displayFromMessage)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(localizationService.GetResource("Products.PriceRangeFrom"), priceFormatter.FormatPrice(finalPrice));
                                                priceModel.PriceValue = finalPrice;
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = priceFormatter.FormatPrice(oldPrice);
                                                    priceModel.Price = priceFormatter.FormatPrice(finalPrice);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = priceFormatter.FormatPrice(finalPrice);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                            }
                                            if (product.IsRental)
                                            {
                                                //rental product
                                                priceModel.OldPrice = priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                                                priceModel.Price = priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                                            }


                                            //property for German market
                                            //we display tax/shipping info only with "shipping enabled" for this product
                                            //we also ensure this it's not free shipping
                                            priceModel.DisplayTaxShippingInfo = catalogSettings.DisplayTaxShippingInfoProductBoxes
                                                && product.IsShipEnabled &&
                                                !product.IsFreeShipping;


                                            //PAngV baseprice (used in Germany)
                                            priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
                                                localizationService, measureService, currencyService, workContext, priceFormatter);
                                        }
                                    }
                                }
                                else
                                {
                                    //hide prices
                                    priceModel.OldPrice = null;
                                    priceModel.Price = null;
                                }

                                #endregion
                            }
                            break;
                    }


                    var limitPurchaseResponse = LimitPurchase(product, productService, orderService, storeContext, workContext);

                    model.LimitPurchaseResponse = limitPurchaseResponse;

                    priceModel.DisableBuyButton = (workContext.CurrentCustomer.IsGuest() && product.DisableBuyButtonForGuestUsers) || limitPurchaseResponse.LimitPurchase;  //NU-90

                    model.ProductPrice = priceModel;

                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture

                    //If a size has been set in the view, we use it in priority
                    int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : mediaSettings.ProductThumbPictureSize;
                    //prepare picture model
                    var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, workContext.WorkingLanguage.Id, webHelper.IsCurrentConnectionSecured(), storeContext.CurrentStore.Id);
                    model.DefaultPictureModel = cacheManager.Get(defaultProductPictureCacheKey, () =>
                    {
                        var picture = pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                        var pictureModel = new PictureModel
                        {
                            ImageUrl = pictureService.GetPictureUrl(picture, pictureSize),
                            FullSizeImageUrl = pictureService.GetPictureUrl(picture)
                        };
                        //"title" attribute
                        pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                            picture.TitleAttribute :
                            string.Format(localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
                        //"alt" attribute
                        pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                            picture.AltAttribute :
                            string.Format(localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);

                        return pictureModel;
                    });

                    #endregion
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(controller, workContext,
                         specificationAttributeService, cacheManager, product);
                }

                //reviews
                model.ReviewOverviewModel = controller.PrepareProductReviewOverviewModel(storeContext, catalogSettings, cacheManager, product);

                models.Add(model);
            }
            return models;
        }


        public static LimitPurchaseResponse LimitPurchase(Product product, IProductService productService, IOrderService orderService, IStoreContext storeContext, IWorkContext workContext)
        {
            var limitPurchase = false;
            var productRelationsToSeeIfWeCanPurchase = productService.GetProductRelationsByRelatedProduct(product.Id, 2);
            string limitMessage = "";
            if (productRelationsToSeeIfWeCanPurchase.Count > 0)
            {
                //does this person have something in the cart that stops us from buying this product?
                var cart = workContext.CurrentCustomer.ShoppingCartItems;
                foreach (var item in cart)
                {
                    foreach (var relation in productRelationsToSeeIfWeCanPurchase)
                    {
                        if (item.Product.Id == relation.ProductId)
                        {
                            //yes they do
                            limitPurchase = true;
                            limitMessage = "Cannot Purchase";
                        }
                    }
                }

                var orders = orderService.SearchOrders(storeId: storeContext.CurrentStore.Id,
                    customerId: workContext.CurrentCustomer.Id);
                foreach (var order in orders)
                {
                    foreach (OrderItem item in order.OrderItems)
                    {
                        foreach (var relation in productRelationsToSeeIfWeCanPurchase)
                        {
                            if (item.Product.Id == relation.ProductId)
                            {
                                limitPurchase = true;
                                limitMessage = "Cannot Purchase";

                                var response1 = new LimitPurchaseResponse();

                                response1.LimitMessage = limitMessage;
                                response1.LimitPurchase = limitPurchase;
                                return response1;
                            }
                        }
                    }
                }
            }
            if (product.LimitPurchase)
            {
                var ordersForProduct = orderService.SearchOrders(storeId: storeContext.CurrentStore.Id,
                    customerId: workContext.CurrentCustomer.Id, productId: product.Id);
                //has this person ordered before?                   
                foreach (var order in ordersForProduct)
                {
                    foreach (OrderItem item in order.OrderItems)
                    {
                        foreach (var specattr in item.Product.ProductSpecificationAttributes)
                        {
                            if (specattr.SpecificationAttributeOption.SpecificationAttribute.Name == "Season")
                            {
                                if (!product.IsReservation && specattr.SpecificationAttributeOption.Name == storeContext.CurrentStore.CurrentSeason)
                                {
                                    limitPurchase = true; //already purchased for this season
                                    limitMessage = "Purchased";
                                }
                            }
                        }
                    }
                }
                //does this person already have it in the cart?
                var cart = workContext.CurrentCustomer.ShoppingCartItems;

                foreach (var item in cart)
                {
                    if (item.Product.Id == product.Id)
                    {
                        limitPurchase = true;
                        limitMessage = "Already in cart";
                    }
                }
                if (!product.IsReservation && ordersForProduct.Count > product.CustomerMaximumQuantity)
                {
                    limitPurchase = true;
                    limitMessage = "Purchased";
                }
                //  }
                if (product.LimitPurchaseType == 2 && storeContext.CurrentStore.Id == 318) //Is A Commuter && FSU store
                {
                    string empId = workContext.CurrentCustomer.GetAttribute<string>("fsueduemplid", storeContext.CurrentStore.Id);
                    string uiId = workContext.CurrentCustomer.GetAttribute<string>("uid", storeContext.CurrentStore.Id);

                    var customerService = EngineContext.Current.Resolve<ICustomerService>();

                    Commuter commuter = customerService.GetCommuterInfoForCustomer(empId, storeContext.CurrentStore.Id);

                    if (commuter == null)
                    {
                        limitPurchase = false;
                    }
                    else
                    {
                        limitPurchase = true;
                        limitMessage = "Commuters Only";
                    }

                }
            }


            var response = new LimitPurchaseResponse();

            response.LimitMessage = limitMessage;
            response.LimitPurchase = limitPurchase;
            return response;
        }

        public static ProductReviewOverviewModel PrepareProductReviewOverviewModel(this Controller controller,
            IStoreContext storeContext,
            CatalogSettings catalogSettings,
            ICacheManager cacheManager,
            Product product)
        {
            ProductReviewOverviewModel productReview;

            if (catalogSettings.ShowProductReviewsPerStore)
            {
                string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, storeContext.CurrentStore.Id);

                productReview = cacheManager.Get(cacheKey, () =>
                {
                    return new ProductReviewOverviewModel
                    {
                        RatingSum = product.ProductReviews
                                .Where(pr => pr.IsApproved && pr.StoreId == storeContext.CurrentStore.Id)
                                .Sum(pr => pr.Rating),
                        TotalReviews = product
                                .ProductReviews
                                .Count(pr => pr.IsApproved && pr.StoreId == storeContext.CurrentStore.Id)
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel()
                {
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews
                };
            }
            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
            }
            return productReview;
        }
    }
}