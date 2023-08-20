using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute parser interface
    /// </summary>
    public partial interface IProductAttributeParser
    {
        #region Product attributes

        /// <summary>
        /// Gets selected product attribute mappings
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected product attribute mappings</returns>
        IList<ProductAttributeMapping> ParseProductAttributeMappings(string attributesXml);

        /// <summary>
        /// Get product attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Product attribute values</returns>
        IList<ProductAttributeValue> ParseProductAttributeValues(string attributesXml);

        /// <summary>
        /// Gets selected product attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="productAttributeMappingId">Product attribute mapping identifier</param>
        /// <returns>Product attribute values</returns>
        IList<string> ParseValues(string attributesXml, int productAttributeMappingId);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="value">Value</param>
        /// <returns>Updated result (XML format)</returns>
        string AddProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping, string value);

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>Updated result (XML format)</returns>
        string RemoveProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping);

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="attributesXml1">The attributes of the first product</param>
        /// <param name="attributesXml2">The attributes of the second product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Result</returns>
        bool AreProductAttributesEqual(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes);

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="pam">Product attribute</param>
        /// <param name="selectedAttributesXml">Selected attributes (XML format)</param>
        /// <returns>Result</returns>
        bool? IsConditionMet(ProductAttributeMapping pam, string selectedAttributesXml);

        /// <summary>
        /// Finds a product attribute combination by attributes stored in XML 
        /// </summary> 
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Found product attribute combination</returns>
        ProductAttributeCombination FindProductAttributeCombination(Product product,
            string attributesXml, bool ignoreNonCombinableAttributes = true);

        /// <summary>
        /// Generate all combinations
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Attribute combinations in XML format</returns>
        IList<string> GenerateAllCombinations(Product product, bool ignoreNonCombinableAttributes = false);

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attrbibutes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        string AddGiftCardAttribute(string attributesXml, string recipientName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage);

        /// <summary>
        /// Get gift card attrbibutes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        void GetGiftCardAttribute(string attributesXml, out string recipientName,
            out string recipientEmail, out string senderName,
            out string senderEmail, out string giftCardMessage);

        #endregion
        #region NU-16

        /// <summary>
        /// Add meal plan attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="mealPlanRecipientAcctNum">Recipient name</param>
        /// <param name="mealPlanRecipientAddress">Recipient email</param>
        /// <param name="mealPlanRecipientEmail">Sender name</param>
        /// <param name="mealPlanRecipientName">Sender email</param>
        /// <param name="mealPlanRecipientPhone">Message</param>
        /// <returns>Attributes</returns>
        string AddMealPlanAttribute(
            string attributes,
            string mealPlanRecipientAcctNum,
            string mealPlanRecipientAddress,
            string mealPlanRecipientEmail,
            string mealPlanRecipientName,
            string mealPlanRecipientPhone);

        /// <summary>
        /// Get meal plan attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="mealPlanRecipientAcctNum">Recipient name</param>
        /// <param name="mealPlanRecipientAddress">Recipient email</param>
        /// <param name="mealPlanRecipientEmail">Sender name</param>
        /// <param name="mealPlanRecipientName">Sender email</param>
        /// <param name="mealPlanRecipientPhone">Message</param>
        void GetMealPlanAttribute(
            string attributes,
            out string mealPlanRecipientAcctNum,
            out string mealPlanRecipientAddress,
            out string mealPlanRecipientEmail,
            out string mealPlanRecipientName,
            out string mealPlanRecipientPhone);

        #endregion
		
		
        #region NU-17
        /// <summary>
        /// Adds Donation attributes
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="donorFirstName"></param>
        /// <param name="donorLastName"></param>
        /// <param name="donorAddress"></param>
        /// <param name="donorAddress2"></param>
        /// <param name="donorCity"></param>
        /// <param name="donorState"></param>
        /// <param name="donorZip"></param>
        /// <param name="donorPhone"></param>
        /// <param name="donorCountry"></param>
        /// <param name="comments"></param>
        /// <param name="notificationEmail"></param>
        /// <param name="onBehalfOfFullName"></param>
        /// <param name="includeGiftAmount"></param>
        /// <param name="donorCompany"></param>
        /// <returns></returns>
        string AddDonationAttribute(
            string attributes,
            string donorFirstName,
            string donorLastName,
            string donorAddress,
            string donorAddress2,
            string donorCity,
            string donorState,
            string donorZip,
            string donorPhone,
            string donorCountry,
            string comments,
            string notificationEmail,
            string onBehalfOfFullName,
            bool includeGiftAmount,
            string donorCompany);

        /// <summary>
        /// Gets donation attributes
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="donorFirstName"></param>
        /// <param name="donorLastName"></param>
        /// <param name="donorAddress"></param>
        /// <param name="donorAddress2"></param>
        /// <param name="donorCity"></param>
        /// <param name="donorState"></param>
        /// <param name="donorZip"></param>
        /// <param name="donorPhone"></param>
        /// <param name="donorCountry"></param>
        /// <param name="comments"></param>
        /// <param name="notificationEmail"></param>
        /// <param name="onBehalfOfFullName"></param>
        /// <param name="includeGiftAmount"></param>
        /// <param name="donorCompany"></param>
        void GetDonationAttribute(
            string attributes,
            out string donorFirstName,
            out string donorLastName,
            out string donorAddress,
            out string donorAddress2,
            out string donorCity,
            out string donorState,
            out string donorZip,
            out string donorPhone,
            out string donorCountry,
            out string comments,
            out string notificationEmail,
            out string onBehalfOfFullName,
            out bool includeGiftAmount,
            out string donorCompany);

        #endregion
		
		
		
		
    }
}
