using System;
using System.Text;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Html;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter
    /// </summary>
    public partial class ProductAttributeFormatter : IProductAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public ProductAttributeFormatter(IWorkContext workContext,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IWebHelper webHelper,
            IPriceCalculationService priceCalculationService,
            ShoppingCartSettings shoppingCartSettings)
        {
            this._workContext = workContext;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._priceFormatter = priceFormatter;
            this._downloadService = downloadService;
            this._webHelper = webHelper;
            this._priceCalculationService = priceCalculationService;
            this._shoppingCartSettings = shoppingCartSettings;
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Attributes</returns>
        public virtual string FormatAttributes(Product product, string attributesXml)
        {
            var customer = _workContext.CurrentCustomer;
            return FormatAttributes(product, attributesXml, customer);
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customer">Customer</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="renderDonationAttributes">A value indicating whether to render donation attributes</param>
        /// <param name="renderMealPlanAttributes">A value indicating whether to render donation attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        public virtual string FormatAttributes(Product product, string attributesXml,
            Customer customer, string serapator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true, 
			bool renderDonationAttributes = true, 	/// NU-17	
			bool renderMealPlanAttributes = true, /// NU-16
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            //attributes
            if (renderProductAttributes)
            {
                var attributes = _productAttributeParser.ParseProductAttributeMappings(attributesXml);
                for (int i = 0; i < attributes.Count; i++)
                {
                    var attribute = attributes[i];
                    var valuesStr = _productAttributeParser.ParseValues(attributesXml, attribute.Id);
                    for (int j = 0; j < valuesStr.Count; j++)
                    {
                        string valueStr = valuesStr[j];
                        string formattedAttribute = string.Empty;
                        if (!attribute.ShouldHaveValues())
                        {
                            //no values
                            if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //multiline textbox
                                var attributeName = attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                formattedAttribute = string.Format("{0}: {1}", attributeName, HtmlHelper.FormatText(valueStr, false, true, false, false, false, false));
                                //we never encode multiline textbox input
                            }
                            else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                //file upload
                                Guid downloadGuid;
                                Guid.TryParse(valueStr, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                                    string attributeText;
                                    var fileName = string.Format("{0}{1}",
                                        download.Filename ?? download.DownloadGuid.ToString(),
                                        download.Extension);
                                    //encode (if required)
                                    if (htmlEncode)
                                        fileName = HttpUtility.HtmlEncode(fileName);
                                    if (allowHyperlinks)
                                    {
                                        //hyperlinks are allowed
                                        var downloadLink = string.Format("{0}download/getfileupload/?downloadId={1}", _webHelper.GetStoreLocation(false), download.DownloadGuid);
                                        attributeText = string.Format("<a href=\"{0}\" class=\"fileuploadattribute\">{1}</a>", downloadLink, fileName);
                                    }
                                    else
                                    {
                                        //hyperlinks aren't allowed
                                        attributeText = fileName;
                                    }
                                    var attributeName = attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                                    //encode (if required)
                                    if (htmlEncode)
                                        attributeName = HttpUtility.HtmlEncode(attributeName);
                                    formattedAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                                }
                            }
                            else
                            {
                                //other attributes (textbox, datepicker)
                                formattedAttribute = string.Format("{0}: {1}", attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), valueStr);
                                //encode (if required)
                                if (htmlEncode)
                                    formattedAttribute = HttpUtility.HtmlEncode(formattedAttribute);
                            }
                        }
                        else
                        {
                            //attributes with values
                            int attributeValueId;
                            if (int.TryParse(valueStr, out attributeValueId))
                            {
                                var attributeValue = _productAttributeService.GetProductAttributeValueById(attributeValueId);
                                if (attributeValue != null)
                                {
                                    if (!product.IsBundleProduct)
                                    {
                                        formattedAttribute = string.Format("{0}: {1}", attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));
                                    }
                                    else
                                    {
                                        formattedAttribute = string.Format("{0}",attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));
                                    }
                                    if (renderPrices)
                                    {
                                        decimal taxRate;
                                        decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                                        decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, customer, out taxRate);
                                        decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                        if (priceAdjustmentBase > 0 && !product.IsBundleProduct)
                                        {
                                            string priceAdjustmentStr = _priceFormatter.FormatPrice(priceAdjustment, false, false);
                                            formattedAttribute += string.Format(" [+{0}]", priceAdjustmentStr);
                                        }
                                        else if (priceAdjustmentBase < decimal.Zero && !product.IsBundleProduct)
                                        {
                                            string priceAdjustmentStr = _priceFormatter.FormatPrice(-priceAdjustment, false, false);
                                            formattedAttribute += string.Format(" [-{0}]", priceAdjustmentStr);
                                        }
                                    }

                                    //display quantity
                                    if (_shoppingCartSettings.RenderAssociatedAttributeValueQuantity &&
                                        attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                                    {
                                        //render only when more than 1
                                        if (attributeValue.Quantity > 1)
                                        {
                                            //TODO localize resource
                                            formattedAttribute += string.Format(" - qty {0}", attributeValue.Quantity);
                                        }
                                    }
                                }
                                //encode (if required)



                                if (htmlEncode && (attribute.AttributeControlType != AttributeControlType.Checkboxes && attribute.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
                                {
                                    formattedAttribute = HttpUtility.HtmlEncode(formattedAttribute);
                                }
                                    
                            }
                        }

                        if (!String.IsNullOrEmpty(formattedAttribute))
                        {
                            if (i != 0 || j != 0)
                                result.Append(serapator);
                            result.Append(formattedAttribute);
                        }
                    }
                }
            }

            //gift cards
            if (renderGiftCardAttributes)
            {
                if (product.IsGiftCard)
                {
                    string giftCardRecipientName;
                    string giftCardRecipientEmail;
                    string giftCardSenderName;
                    string giftCardSenderEmail;
                    string giftCardMessage;
                    _productAttributeParser.GetGiftCardAttribute(attributesXml, out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    //sender
                    var giftCardFrom = product.GiftCardType == GiftCardType.Virtual ?
                        string.Format(_localizationService.GetResource("GiftCardAttribute.From.Virtual"), giftCardSenderName, giftCardSenderEmail) :
                        string.Format(_localizationService.GetResource("GiftCardAttribute.From.Physical"), giftCardSenderName);
                    //recipient
                    var giftCardFor = product.GiftCardType == GiftCardType.Virtual ?
                        string.Format(_localizationService.GetResource("GiftCardAttribute.For.Virtual"), giftCardRecipientName, giftCardRecipientEmail) :
                        string.Format(_localizationService.GetResource("GiftCardAttribute.For.Physical"), giftCardRecipientName);

                    //encode (if required)
                    if (htmlEncode)
                    {
                        giftCardFrom = HttpUtility.HtmlEncode(giftCardFrom);
                        giftCardFor = HttpUtility.HtmlEncode(giftCardFor);
                    }

                    if (!String.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append(serapator);
                    }
                    result.Append(giftCardFrom);
                    result.Append(serapator);
                    result.Append(giftCardFor);
                }
            }
			#region NU-16
            if (renderMealPlanAttributes)
            {
                if (product.IsMealPlan && product.ShowStandardMealPlanFields)
                {
                    string mealPlanRecipientAcctNum;
                    string mealPlanRecipientAddress;
                    string mealPlanRecipientEmail;
                    string mealPlanRecipientName;
                    string mealPlanRecipientPhone;
                    _productAttributeParser.GetMealPlanAttribute(attributesXml, out mealPlanRecipientAcctNum, out mealPlanRecipientAddress,
                        out mealPlanRecipientEmail, out mealPlanRecipientName, out mealPlanRecipientPhone);

                    if (!String.IsNullOrEmpty(result.ToString()))
                        result.Append(serapator);

                    var name = string.Format("Recipient Name: {0}", mealPlanRecipientName);
                    var acctNum = string.Format("Student Acct No.: {0}", mealPlanRecipientAcctNum);
                    if (!string.IsNullOrEmpty(mealPlanRecipientName) || !string.IsNullOrEmpty(mealPlanRecipientAcctNum)) // SE-65 issue prevented of not showing unecessary error if allowed quatities is not given
                    {
                        if (htmlEncode)
                        {
                            result.Append(HttpUtility.HtmlEncode(name));
                            result.Append(serapator);
                            result.Append(HttpUtility.HtmlEncode(acctNum));
                        }
                        else
                        {
                            result.Append(name);
                            result.Append(serapator);
                            result.Append(acctNum);
                        }
                    }
                }
            }
			#endregion

            #region NU-17
            if (renderDonationAttributes)
            {
                if (product.IsDonation) 
                {
                    string donorFirstName;
                    string donorLastName;
                    string donorAddress;
                    string donorAddress2;
                    string donorCity;
                    string donorState;
                    string donorZip;
                    string donorPhone;
                    string donorCountry;
                    string comments;
                    string notificationEmail;
                    string onBehalfOfFullName;
                    string donorCompany;
                    bool includeGiftAmount;

                    _productAttributeParser.GetDonationAttribute(attributesXml, out  donorFirstName, out  donorLastName, out  donorAddress, out  donorAddress2, out  donorCity, out  donorState,
                        out  donorZip, out  donorPhone, out  donorCountry, out  comments, out  notificationEmail, out  onBehalfOfFullName, out includeGiftAmount, out donorCompany);

                    if (!String.IsNullOrEmpty(result.ToString()))
                        result.Append(serapator);

                    var firstName = string.Format("Donor First Name: {0}", donorFirstName);
                    var lastName = string.Format("Donor Last Name: {0}", donorLastName);
                    var company = string.Format("Donor Company: {0}", donorCompany);
                    var address = string.Format("Donor Address: {0}", donorAddress);
                    var address2 = string.Format("Donor Address 2: {0}", donorAddress2);
                    var city = string.Format("Donor City: {0}", donorCity);
                    var state = string.Format("Donor State: {0}", donorState);
                    var zip = string.Format("Donor Zip: {0}", donorZip);
                    var phone = string.Format("Donor Phone: {0}", donorPhone);
                    var country = string.Format("Donor Country: {0}", donorCountry);

                    var comments1 = "";
                    if (comments != "")
                    {
                        comments1 = string.Format("Order Comments: {0}", comments);
                    }

                    var notificationEmail1 = "";
                    if (notificationEmail != "")
                    {
                        notificationEmail1 = string.Format("Notification Email: {0}", notificationEmail);
                    }

                    var onBehalfOfFullName1 = "";
                    if (onBehalfOfFullName != "")
                    {
                        onBehalfOfFullName1 = string.Format("On Behalf of: {0}", onBehalfOfFullName);
                    }

                    var includeGiftAmount1 = "";
                    if (includeGiftAmount)
                    {
                        includeGiftAmount1 = string.Format("Include Gift Amount In Message");
                    }

                    if (htmlEncode)
                    {
                        result.Append(HttpUtility.HtmlEncode(firstName));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(lastName));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(company));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(address));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(address2));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(city));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(state));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(zip));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(phone));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(country));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(notificationEmail1));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(onBehalfOfFullName1));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(includeGiftAmount1));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(comments1));


                    }
                    else
                    {
                        result.Append(firstName);
                        result.Append(serapator);
                        result.Append(lastName);
                        result.Append(serapator);
                        result.Append(company);
                        result.Append(serapator);
                        result.Append(address);
                        result.Append(serapator);
                        result.Append(address2);
                        result.Append(serapator);
                        result.Append(city);
                        result.Append(serapator);
                        result.Append(state);
                        result.Append(serapator);
                        result.Append(zip);
                        result.Append(serapator);
                        result.Append(phone);
                        result.Append(serapator);
                        result.Append(country);
                        result.Append(serapator);
                        result.Append(onBehalfOfFullName1);
                        result.Append(serapator);
                        result.Append(notificationEmail1);
                        result.Append(serapator);
                        result.Append(includeGiftAmount1);
                        result.Append(serapator);
                        result.Append(comments1);
                    }

                }
            }
            #endregion
            return result.ToString();
        }
    }
}
