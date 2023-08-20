using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Nop.Services.Helpers;
using System.Collections.Concurrent;
using Nop.Services.Orders;
using Nop.Services.Customers;
using Nop.Core.Domain.KitchenProduction;
using System.Data;
using Nop.Services.Security;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ProductEditorSettings _productEditorSettings;
        private readonly IVendorService _vendorService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IShippingService _shippingService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IEncryptionService _encryptionService;
        private readonly IMeasureService _measureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGlCodeService _glCodeService;

        private readonly CatalogSettings _catalogSettings;
        private readonly IOrderService _orderService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
             IOrderService orderService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IGenericAttributeService genericAttributeService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreService storeService,
            IWorkContext workContext,
            ProductEditorSettings productEditorSettings,
            IVendorService vendorService,
            IProductTemplateService productTemplateService,
            IShippingService shippingService,
            ITaxCategoryService taxCategoryService,
            IMeasureService measureService,
            IGlCodeService glCodeService,
            CatalogSettings catalogSettings,
           IStateProvinceService stateProvinceService,
           ICustomerService customerService,
           IEncryptionService encryptionService)
        {
            this._encryptionService = encryptionService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._productEditorSettings = productEditorSettings;
            this._vendorService = vendorService;
            this._productTemplateService = productTemplateService;
            this._shippingService = shippingService;
            this._taxCategoryService = taxCategoryService;
            this._measureService = measureService;
            this._glCodeService = glCodeService;
            this._catalogSettings = catalogSettings;
            this._orderService = orderService;
            this._stateProvinceService = stateProvinceService;
            this._customerService = customerService;
        }

        #endregion

        #region Utilities

        protected virtual void WriteCategories(XmlWriter xmlWriter, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            if (categories != null && categories.Any())
            {
                foreach (var category in categories)
                {
                    xmlWriter.WriteStartElement("Category");
                    xmlWriter.WriteElementString("Id", null, category.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, category.Name);
                    xmlWriter.WriteElementString("Description", null, category.Description);
                    xmlWriter.WriteElementString("CategoryTemplateId", null, category.CategoryTemplateId.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, category.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, category.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, category.MetaTitle);
                    xmlWriter.WriteElementString("SeName", null, category.GetSeName(0));
                    xmlWriter.WriteElementString("ParentCategoryId", null, category.ParentCategoryId.ToString());
                    xmlWriter.WriteElementString("PictureId", null, category.PictureId.ToString());
                    xmlWriter.WriteElementString("PageSize", null, category.PageSize.ToString());
                    xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, category.AllowCustomersToSelectPageSize.ToString());
                    xmlWriter.WriteElementString("PageSizeOptions", null, category.PageSizeOptions);
                    xmlWriter.WriteElementString("PriceRanges", null, category.PriceRanges);
                    xmlWriter.WriteElementString("ShowOnHomePage", null, category.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("IncludeInTopMenu", null, category.IncludeInTopMenu.ToString());
                    xmlWriter.WriteElementString("Published", null, category.Published.ToString());
                    xmlWriter.WriteElementString("Deleted", null, category.Deleted.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, category.DisplayOrder.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, category.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, category.UpdatedOnUtc.ToString());


                    xmlWriter.WriteStartElement("Products");
                    var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, showHidden: true);
                    foreach (var productCategory in productCategories)
                    {
                        var product = productCategory.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productCategory.ProductId.ToString());
                            xmlWriter.WriteElementString("ProductName", null, product.Name);
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SubCategories");
                    WriteCategories(xmlWriter, category.Id);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }
        }

        protected virtual void SetCaptionStyle(ExcelStyle style)
        {
            style.Fill.PatternType = ExcelFillStyle.Solid;
            style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
            style.Font.Bold = true;
        }

        /// <summary>
        /// Returns the path to the image file by ID
        /// </summary>
        /// <param name="pictureId">Picture ID</param>
        /// <returns>Path to the image file</returns>
        protected virtual string GetPictures(int pictureId)
        {
            var picture = _pictureService.GetPictureById(pictureId);
            return _pictureService.GetThumbLocalPath(picture);
        }

        /// <summary>
        /// Returns the list of categories for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of categories</returns>
        protected virtual string GetCategories(Product product)
        {
            string categoryNames = null;
            foreach (var pc in _categoryService.GetProductCategoriesByProductId(product.Id))
            {
                categoryNames += pc.Category.Name;
                categoryNames += ";";
            }
            return categoryNames;
        }

        /// <summary>
        /// Returns the list of manufacturer for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of manufacturer</returns>
        protected virtual string GetManufacturers(Product product)
        {
            string manufacturerNames = null;
            foreach (var pm in _manufacturerService.GetProductManufacturersByProductId(product.Id))
            {
                manufacturerNames += pm.Manufacturer.Name;
                manufacturerNames += ";";
            }
            return manufacturerNames;
        }

        /// <summary>
        /// Returns the three first image associated with the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>three first image</returns>
        protected virtual string[] GetPictures(Product product)
        {
            //pictures (up to 3 pictures)
            string picture1 = null;
            string picture2 = null;
            string picture3 = null;
            var pictures = _pictureService.GetPicturesByProductId(product.Id, 3);
            for (var i = 0; i < pictures.Count; i++)
            {
                var pictureLocalPath = _pictureService.GetThumbLocalPath(pictures[i]);
                switch (i)
                {
                    case 0:
                        picture1 = pictureLocalPath;
                        break;
                    case 1:
                        picture2 = pictureLocalPath;
                        break;
                    case 2:
                        picture3 = pictureLocalPath;
                        break;
                }
            }
            return new[] { picture1, picture2, picture3 };
        }

        private bool IgnoreExportPoductProperty(Func<ProductEditorSettings, bool> func)
        {
            var productAdvancedMode = _workContext.CurrentCustomer.GetAttribute<bool>("product-advanced-mode");
            return !productAdvancedMode && !func(_productEditorSettings);
        }

        /// <summary>
        /// Export objects to XLSX
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="properties">Class access to the object through its properties</param>
        /// <param name="itemsToExport">The objects to export</param>
        /// <returns></returns>
        protected virtual byte[] ExportToXlsx<T>(PropertyByName<T>[] properties, IEnumerable<T> itemsToExport)
        {
            using (var stream = new MemoryStream())
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handle to the existing worksheet
                    var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(T).Name);
                    //create Headers and format them 

                    var manager = new PropertyManager<T>(properties.Where(p => !p.Ignore).ToArray());
                    manager.WriteCaption(worksheet, SetCaptionStyle);

                    var row = 2;
                    foreach (var items in itemsToExport)
                    {
                        manager.CurrentObject = items;
                        manager.WriteToXlsx(worksheet, row++);
                    }
                    string DateCellFormat = "mm/dd/yyyy";
                    using (ExcelRange Rng = worksheet.Cells["AF:AF"])
                    {
                        Rng.Style.Numberformat.Format = DateCellFormat;
                    }

                    string currencyFormat = "$ #,###,###.00";
                    using (ExcelRange Rng = worksheet.Cells["E:E"])
                    {
                        Rng.Style.Numberformat.Format = currencyFormat;
                    }

                    xlPackage.Save();
                }
                return stream.ToArray();
            }
        }

        protected virtual byte[] ExportKitchenProdutionToXlsx<T>(PropertyByName<T>[] properties, IEnumerable<T> itemsToExport)
        {
            using (var stream = new MemoryStream())
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handle to the existing worksheet
                    var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(T).Name);
                    //create Headers and format them 

                    var manager = new PropertyManager<T>(properties.Where(p => !p.Ignore).ToArray());
                    manager.WriteCaption(worksheet, SetCaptionStyle);

                    var row = 2;
                    foreach (var items in itemsToExport)
                    {
                        manager.CurrentObject = items;
                        manager.WriteToXlsx(worksheet, row++);
                    }
                    string DateCellFormat = "mm/dd/yyyy";
                    using (ExcelRange Rng = worksheet.Cells["A:A"])
                    {
                        Rng.Style.Numberformat.Format = DateCellFormat;
                    }

                    using (ExcelRange Rng = worksheet.Cells["E:E"])
                    {
                        Rng.Style.Numberformat.Format = DateCellFormat;
                    }

                    //string currencyFormat = "$ #,###,###.00";
                    //using (ExcelRange Rng = worksheet.Cells["E:E"])
                    //{
                    //    Rng.Style.Numberformat.Format = currencyFormat;
                    //}

                    xlPackage.Save();
                }
                return stream.ToArray();
            }
        }

        private byte[] ExportProductsToXlsxWithAttributes(PropertyByName<Product>[] properties, IEnumerable<Product> itemsToExport)
        {
            var attributeProperties = new[]
            {
                new PropertyByName<ExportProductAttribute>("AttributeId", p => p.AttributeId),
                new PropertyByName<ExportProductAttribute>("AttributeName", p => p.AttributeName),
                new PropertyByName<ExportProductAttribute>("AttributeTextPrompt", p => p.AttributeTextPrompt),
                new PropertyByName<ExportProductAttribute>("AttributeIsRequired", p => p.AttributeIsRequired),
                new PropertyByName<ExportProductAttribute>("AttributeControlType", p => p.AttributeControlTypeId)
                {
                    DropDownElements = AttributeControlType.TextBox.ToSelectList(useLocalization: false)
                },
                new PropertyByName<ExportProductAttribute>("AttributeDisplayOrder", p => p.AttributeDisplayOrder),
                new PropertyByName<ExportProductAttribute>("ProductAttributeValueId", p => p.Id),
                new PropertyByName<ExportProductAttribute>("ValueName", p => p.Name),
                new PropertyByName<ExportProductAttribute>("AttributeValueType", p => p.AttributeValueTypeId)
                {
                    DropDownElements = AttributeValueType.Simple.ToSelectList(useLocalization: false)
                },
                new PropertyByName<ExportProductAttribute>("AssociatedProductId", p => p.AssociatedProductId),
                new PropertyByName<ExportProductAttribute>("ColorSquaresRgb", p => p.ColorSquaresRgb),
                new PropertyByName<ExportProductAttribute>("ImageSquaresPictureId", p => p.ImageSquaresPictureId),
                new PropertyByName<ExportProductAttribute>("PriceAdjustment", p => p.PriceAdjustment),
                new PropertyByName<ExportProductAttribute>("WeightAdjustment", p => p.WeightAdjustment),
                new PropertyByName<ExportProductAttribute>("Cost", p => p.Cost),
                new PropertyByName<ExportProductAttribute>("Quantity", p => p.Quantity),
                new PropertyByName<ExportProductAttribute>("IsPreSelected", p => p.IsPreSelected),
                new PropertyByName<ExportProductAttribute>("DisplayOrder", p => p.DisplayOrder),
                new PropertyByName<ExportProductAttribute>("PictureId", p => p.PictureId)
            };

            var attributeManager = new PropertyManager<ExportProductAttribute>(attributeProperties);

            using (var stream = new MemoryStream())
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handle to the existing worksheet
                    var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(Product).Name);
                    //create Headers and format them 

                    var manager = new PropertyManager<Product>(properties.Where(p => !p.Ignore).ToArray());
                    manager.WriteCaption(worksheet, SetCaptionStyle);

                    var row = 2;
                    foreach (var item in itemsToExport)
                    {
                        manager.CurrentObject = item;
                        manager.WriteToXlsx(worksheet, row++);

                        var attributes = item.ProductAttributeMappings.SelectMany(pam => pam.ProductAttributeValues.Select(pav => new ExportProductAttribute
                        {
                            AttributeId = pam.ProductAttribute.Id,
                            AttributeName = pam.ProductAttribute.Name,
                            AttributeTextPrompt = pam.TextPrompt,
                            AttributeIsRequired = pam.IsRequired,
                            AttributeControlTypeId = pam.AttributeControlTypeId,
                            AssociatedProductId = pav.AssociatedProductId,
                            AttributeDisplayOrder = pam.DisplayOrder,
                            Id = pav.Id,
                            Name = pav.Name,
                            AttributeValueTypeId = pav.AttributeValueTypeId,
                            ColorSquaresRgb = pav.ColorSquaresRgb,
                            ImageSquaresPictureId = pav.ImageSquaresPictureId,
                            PriceAdjustment = pav.PriceAdjustment,
                            WeightAdjustment = pav.WeightAdjustment,
                            Cost = pav.Cost,
                            Quantity = pav.Quantity,
                            IsPreSelected = pav.IsPreSelected,
                            DisplayOrder = pav.DisplayOrder,
                            PictureId = pav.PictureId
                        })).ToList();

                        attributes.AddRange(item.ProductAttributeMappings.Where(pam => !pam.ProductAttributeValues.Any()).Select(pam => new ExportProductAttribute
                        {
                            AttributeId = pam.ProductAttribute.Id,
                            AttributeName = pam.ProductAttribute.Name,
                            AttributeTextPrompt = pam.TextPrompt,
                            AttributeIsRequired = pam.IsRequired,
                            AttributeControlTypeId = pam.AttributeControlTypeId
                        }));

                        if (!attributes.Any())
                            continue;

                        attributeManager.WriteCaption(worksheet, SetCaptionStyle, row, ExportProductAttribute.ProducAttributeCellOffset);
                        worksheet.Row(row).OutlineLevel = 1;
                        worksheet.Row(row).Collapsed = true;

                        foreach (var exportProducAttribute in attributes)
                        {
                            row++;
                            attributeManager.CurrentObject = exportProducAttribute;
                            attributeManager.WriteToXlsx(worksheet, row, ExportProductAttribute.ProducAttributeCellOffset);
                            worksheet.Row(row).OutlineLevel = 1;
                            worksheet.Row(row).Collapsed = true;
                        }

                        row++;
                    }

                    xlPackage.Save();
                }
                return stream.ToArray();
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportManufacturersToXml(IList<Manufacturer> manufacturers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Manufacturers");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var manufacturer in manufacturers)
            {
                xmlWriter.WriteStartElement("Manufacturer");

                xmlWriter.WriteElementString("ManufacturerId", null, manufacturer.Id.ToString());
                xmlWriter.WriteElementString("Name", null, manufacturer.Name);
                xmlWriter.WriteElementString("Description", null, manufacturer.Description);
                xmlWriter.WriteElementString("ManufacturerTemplateId", null, manufacturer.ManufacturerTemplateId.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, manufacturer.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, manufacturer.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, manufacturer.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, manufacturer.GetSeName(0));
                xmlWriter.WriteElementString("PictureId", null, manufacturer.PictureId.ToString());
                xmlWriter.WriteElementString("PageSize", null, manufacturer.PageSize.ToString());
                xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, manufacturer.AllowCustomersToSelectPageSize.ToString());
                xmlWriter.WriteElementString("PageSizeOptions", null, manufacturer.PageSizeOptions);
                xmlWriter.WriteElementString("PriceRanges", null, manufacturer.PriceRanges);
                xmlWriter.WriteElementString("Published", null, manufacturer.Published.ToString());
                xmlWriter.WriteElementString("Deleted", null, manufacturer.Deleted.ToString());
                xmlWriter.WriteElementString("DisplayOrder", null, manufacturer.DisplayOrder.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, manufacturer.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, manufacturer.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("Products");
                var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturer.Id, showHidden: true);
                if (productManufacturers != null)
                {
                    foreach (var productManufacturer in productManufacturers)
                    {
                        var product = productManufacturer.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductManufacturer");
                            xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productManufacturer.ProductId.ToString());
                            xmlWriter.WriteElementString("ProductName", null, product.Name);
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();


                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export manufacturers to XLSX
        /// </summary>
        /// <param name="manufacturers">Manufactures</param>
        public virtual byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<Manufacturer>("Id", p => p.Id),
                new PropertyByName<Manufacturer>("Name", p => p.Name),
                new PropertyByName<Manufacturer>("Description", p => p.Description),
                new PropertyByName<Manufacturer>("ManufacturerTemplateId", p => p.ManufacturerTemplateId),
                new PropertyByName<Manufacturer>("MetaKeywords", p => p.MetaKeywords),
                new PropertyByName<Manufacturer>("MetaDescription", p => p.MetaDescription),
                new PropertyByName<Manufacturer>("MetaTitle", p => p.MetaTitle),
                new PropertyByName<Manufacturer>("SeName", p => p.GetSeName(0)),
                new PropertyByName<Manufacturer>("Picture", p => GetPictures(p.PictureId)),
                new PropertyByName<Manufacturer>("PageSize", p => p.PageSize),
                new PropertyByName<Manufacturer>("AllowCustomersToSelectPageSize", p => p.AllowCustomersToSelectPageSize),
                new PropertyByName<Manufacturer>("PageSizeOptions", p => p.PageSizeOptions),
                new PropertyByName<Manufacturer>("PriceRanges", p => p.PriceRanges),
                new PropertyByName<Manufacturer>("Published", p => p.Published),
                new PropertyByName<Manufacturer>("DisplayOrder", p => p.DisplayOrder)
            };

            return ExportToXlsx(properties, manufacturers);
        }

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCategoriesToXml()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Categories");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
            WriteCategories(xmlWriter, 0);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export categories to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        public virtual byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<Category>("Id", p => p.Id),
                new PropertyByName<Category>("Name", p => p.Name),
                new PropertyByName<Category>("Description", p => p.Description),
                new PropertyByName<Category>("CategoryTemplateId", p => p.CategoryTemplateId),
                new PropertyByName<Category>("MetaKeywords", p => p.MetaKeywords),
                new PropertyByName<Category>("MetaDescription", p => p.MetaDescription),
                new PropertyByName<Category>("MetaTitle", p => p.MetaTitle),
                new PropertyByName<Category>("SeName", p => p.GetSeName(0)),
                new PropertyByName<Category>("ParentCategoryId", p => p.ParentCategoryId),
                new PropertyByName<Category>("Picture", p => GetPictures(p.PictureId)),
                new PropertyByName<Category>("PageSize", p => p.PageSize),
                new PropertyByName<Category>("AllowCustomersToSelectPageSize", p => p.AllowCustomersToSelectPageSize),
                new PropertyByName<Category>("PageSizeOptions", p => p.PageSizeOptions),
                new PropertyByName<Category>("PriceRanges", p => p.PriceRanges),
                new PropertyByName<Category>("ShowOnHomePage", p => p.ShowOnHomePage),
                new PropertyByName<Category>("IncludeInTopMenu", p => p.IncludeInTopMenu),
                new PropertyByName<Category>("Published", p => p.Published),
                new PropertyByName<Category>("DisplayOrder", p => p.DisplayOrder)
            };
            return ExportToXlsx(properties, categories);
        }

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportProductsToXml(IList<Product> products)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Products");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var product in products)
            {
                xmlWriter.WriteStartElement("Product");

                xmlWriter.WriteElementString("ProductId", null, product.Id.ToString());
                xmlWriter.WriteElementString("ProductTypeId", null, product.ProductTypeId.ToString());
                xmlWriter.WriteElementString("ParentGroupedProductId", null, product.ParentGroupedProductId.ToString());
                xmlWriter.WriteElementString("VisibleIndividually", null, product.VisibleIndividually.ToString());
                xmlWriter.WriteElementString("Name", null, product.Name);
                xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
                xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
                xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
                xmlWriter.WriteElementString("VendorId", null, product.VendorId.ToString());
                xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
                xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, product.GetSeName(0));
                xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
                xmlWriter.WriteElementString("SKU", null, product.Sku);
                xmlWriter.WriteElementString("ManufacturerPartNumber", null, product.ManufacturerPartNumber);
                xmlWriter.WriteElementString("Gtin", null, product.Gtin);
                xmlWriter.WriteElementString("IsGiftCard", null, product.IsGiftCard.ToString());
                xmlWriter.WriteElementString("IsMealPlan", null, product.IsMealPlan.ToString());	/// NU-16
                xmlWriter.WriteElementString("GiftCardType", null, product.GiftCardType.ToString());
                xmlWriter.WriteElementString("OverriddenGiftCardAmount", null, product.OverriddenGiftCardAmount.HasValue ? product.OverriddenGiftCardAmount.ToString() : String.Empty);
                xmlWriter.WriteElementString("RequireOtherProducts", null, product.RequireOtherProducts.ToString());
                xmlWriter.WriteElementString("RequiredProductIds", null, product.RequiredProductIds);
                xmlWriter.WriteElementString("AutomaticallyAddRequiredProducts", null, product.AutomaticallyAddRequiredProducts.ToString());
                xmlWriter.WriteElementString("IsDownload", null, product.IsDownload.ToString());
                xmlWriter.WriteElementString("DownloadId", null, product.DownloadId.ToString());
                xmlWriter.WriteElementString("UnlimitedDownloads", null, product.UnlimitedDownloads.ToString());
                xmlWriter.WriteElementString("MaxNumberOfDownloads", null, product.MaxNumberOfDownloads.ToString());
                if (product.DownloadExpirationDays.HasValue)
                    xmlWriter.WriteElementString("DownloadExpirationDays", null, product.DownloadExpirationDays.ToString());
                else
                    xmlWriter.WriteElementString("DownloadExpirationDays", null, String.Empty);
                xmlWriter.WriteElementString("DownloadActivationType", null, product.DownloadActivationType.ToString());
                xmlWriter.WriteElementString("HasSampleDownload", null, product.HasSampleDownload.ToString());
                xmlWriter.WriteElementString("SampleDownloadId", null, product.SampleDownloadId.ToString());
                xmlWriter.WriteElementString("HasUserAgreement", null, product.HasUserAgreement.ToString());
                xmlWriter.WriteElementString("UserAgreementText", null, product.UserAgreementText);
                xmlWriter.WriteElementString("IsRecurring", null, product.IsRecurring.ToString());
                xmlWriter.WriteElementString("RecurringCycleLength", null, product.RecurringCycleLength.ToString());
                xmlWriter.WriteElementString("RecurringCyclePeriodId", null, product.RecurringCyclePeriodId.ToString());
                xmlWriter.WriteElementString("RecurringTotalCycles", null, product.RecurringTotalCycles.ToString());
                xmlWriter.WriteElementString("IsRental", null, product.IsRental.ToString());
                xmlWriter.WriteElementString("RentalPriceLength", null, product.RentalPriceLength.ToString());
                xmlWriter.WriteElementString("RentalPricePeriodId", null, product.RentalPricePeriodId.ToString());
                xmlWriter.WriteElementString("IsShipEnabled", null, product.IsShipEnabled.ToString());
                xmlWriter.WriteElementString("IsFreeShipping", null, product.IsFreeShipping.ToString());
                xmlWriter.WriteElementString("ShipSeparately", null, product.ShipSeparately.ToString());
                xmlWriter.WriteElementString("AdditionalShippingCharge", null, product.AdditionalShippingCharge.ToString());
                xmlWriter.WriteElementString("DeliveryDateId", null, product.DeliveryDateId.ToString());
                xmlWriter.WriteElementString("IsTaxExempt", null, product.IsTaxExempt.ToString());
                xmlWriter.WriteElementString("TaxCategoryId", null, product.TaxCategoryId.ToString());
                xmlWriter.WriteElementString("IsTelecommunicationsOrBroadcastingOrElectronicServices", null, product.IsTelecommunicationsOrBroadcastingOrElectronicServices.ToString());
                xmlWriter.WriteElementString("ManageInventoryMethodId", null, product.ManageInventoryMethodId.ToString());
                xmlWriter.WriteElementString("UseMultipleWarehouses", null, product.UseMultipleWarehouses.ToString());
                xmlWriter.WriteElementString("WarehouseId", null, product.WarehouseId.ToString());
                xmlWriter.WriteElementString("StockQuantity", null, product.StockQuantity.ToString());
                xmlWriter.WriteElementString("DisplayStockAvailability", null, product.DisplayStockAvailability.ToString());
                xmlWriter.WriteElementString("DisplayStockQuantity", null, product.DisplayStockQuantity.ToString());
                xmlWriter.WriteElementString("MinStockQuantity", null, product.MinStockQuantity.ToString());
                xmlWriter.WriteElementString("LowStockActivityId", null, product.LowStockActivityId.ToString());
                xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, product.NotifyAdminForQuantityBelow.ToString());
                xmlWriter.WriteElementString("BackorderModeId", null, product.BackorderModeId.ToString());
                xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, product.AllowBackInStockSubscriptions.ToString());
                xmlWriter.WriteElementString("OrderMinimumQuantity", null, product.OrderMinimumQuantity.ToString());
                xmlWriter.WriteElementString("OrderMaximumQuantity", null, product.OrderMaximumQuantity.ToString());
                xmlWriter.WriteElementString("AllowedQuantities", null, product.AllowedQuantities);
                xmlWriter.WriteElementString("AllowAddingOnlyExistingAttributeCombinations", null, product.AllowAddingOnlyExistingAttributeCombinations.ToString());
                xmlWriter.WriteElementString("NotReturnable", null, product.NotReturnable.ToString());
                xmlWriter.WriteElementString("DisableBuyButton", null, product.DisableBuyButton.ToString());
                xmlWriter.WriteElementString("DisableWishlistButton", null, product.DisableWishlistButton.ToString());
                xmlWriter.WriteElementString("AvailableForPreOrder", null, product.AvailableForPreOrder.ToString());
                xmlWriter.WriteElementString("PreOrderAvailabilityStartDateTimeUtc", null, product.PreOrderAvailabilityStartDateTimeUtc.HasValue ? product.PreOrderAvailabilityStartDateTimeUtc.ToString() : String.Empty);
                xmlWriter.WriteElementString("CallForPrice", null, product.CallForPrice.ToString());
                xmlWriter.WriteElementString("Price", null, product.Price.ToString());
                xmlWriter.WriteElementString("OldPrice", null, product.OldPrice.ToString());
                xmlWriter.WriteElementString("ProductCost", null, product.ProductCost.ToString());
                xmlWriter.WriteElementString("SpecialPrice", null, product.SpecialPrice.HasValue ? product.SpecialPrice.ToString() : String.Empty);
                xmlWriter.WriteElementString("SpecialPriceStartDateTimeUtc", null, product.SpecialPriceStartDateTimeUtc.HasValue ? product.SpecialPriceStartDateTimeUtc.ToString() : String.Empty);
                xmlWriter.WriteElementString("SpecialPriceEndDateTimeUtc", null, product.SpecialPriceEndDateTimeUtc.HasValue ? product.SpecialPriceEndDateTimeUtc.ToString() : String.Empty);
                xmlWriter.WriteElementString("CustomerEntersPrice", null, product.CustomerEntersPrice.ToString());
                xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, product.MinimumCustomerEnteredPrice.ToString());
                xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, product.MaximumCustomerEnteredPrice.ToString());
                xmlWriter.WriteElementString("BasepriceEnabled", null, product.BasepriceEnabled.ToString());
                xmlWriter.WriteElementString("BasepriceAmount", null, product.BasepriceAmount.ToString());
                xmlWriter.WriteElementString("BasepriceUnitId", null, product.BasepriceUnitId.ToString());
                xmlWriter.WriteElementString("BasepriceBaseAmount", null, product.BasepriceBaseAmount.ToString());
                xmlWriter.WriteElementString("BasepriceBaseUnitId", null, product.BasepriceBaseUnitId.ToString());
                xmlWriter.WriteElementString("MarkAsNew", null, product.MarkAsNew.ToString());
                xmlWriter.WriteElementString("MarkAsNewStartDateTimeUtc", null, product.MarkAsNewStartDateTimeUtc.HasValue ? product.MarkAsNewStartDateTimeUtc.ToString() : String.Empty);
                xmlWriter.WriteElementString("MarkAsNewEndDateTimeUtc", null, product.MarkAsNewEndDateTimeUtc.HasValue ? product.MarkAsNewEndDateTimeUtc.ToString() : String.Empty);
                xmlWriter.WriteElementString("Weight", null, product.Weight.ToString());
                xmlWriter.WriteElementString("Length", null, product.Length.ToString());
                xmlWriter.WriteElementString("Width", null, product.Width.ToString());
                xmlWriter.WriteElementString("Height", null, product.Height.ToString());
                xmlWriter.WriteElementString("Published", null, product.Published.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());
                #region NU-18
                xmlWriter.WriteElementString("ProductionDaysLead", null, product.ProductionDaysLead.ToString());
                xmlWriter.WriteElementString("ProductionHoursLead", null, product.ProductionHoursLead.ToString());
                xmlWriter.WriteElementString("ProductionMinutesLead", null, product.ProductionMinutesLead.ToString());
                #endregion



                xmlWriter.WriteStartElement("ProductDiscounts");
                var discounts = product.AppliedDiscounts;
                foreach (var discount in discounts)
                {
                    xmlWriter.WriteStartElement("Discount");
                    xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, discount.Name);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();


                xmlWriter.WriteStartElement("TierPrices");
                var tierPrices = product.TierPrices;
                foreach (var tierPrice in tierPrices)
                {
                    xmlWriter.WriteStartElement("TierPrice");
                    xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
                    xmlWriter.WriteElementString("StoreId", null, tierPrice.StoreId.ToString());
                    xmlWriter.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
                    xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
                    xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductAttributes");
                var productAttributMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var productAttributeMapping in productAttributMappings)
                {
                    xmlWriter.WriteStartElement("ProductAttributeMapping");
                    xmlWriter.WriteElementString("ProductAttributeMappingId", null, productAttributeMapping.Id.ToString());
                    xmlWriter.WriteElementString("ProductAttributeId", null, productAttributeMapping.ProductAttributeId.ToString());
                    xmlWriter.WriteElementString("ProductAttributeName", null, productAttributeMapping.ProductAttribute.Name);
                    xmlWriter.WriteElementString("TextPrompt", null, productAttributeMapping.TextPrompt);
                    xmlWriter.WriteElementString("IsRequired", null, productAttributeMapping.IsRequired.ToString());
                    xmlWriter.WriteElementString("AttributeControlTypeId", null, productAttributeMapping.AttributeControlTypeId.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productAttributeMapping.DisplayOrder.ToString());
                    //validation rules
                    if (productAttributeMapping.ValidationRulesAllowed())
                    {
                        if (productAttributeMapping.ValidationMinLength.HasValue)
                        {
                            xmlWriter.WriteElementString("ValidationMinLength", null, productAttributeMapping.ValidationMinLength.Value.ToString());
                        }
                        if (productAttributeMapping.ValidationMaxLength.HasValue)
                        {
                            xmlWriter.WriteElementString("ValidationMaxLength", null, productAttributeMapping.ValidationMaxLength.Value.ToString());
                        }
                        if (String.IsNullOrEmpty(productAttributeMapping.ValidationFileAllowedExtensions))
                        {
                            xmlWriter.WriteElementString("ValidationFileAllowedExtensions", null, productAttributeMapping.ValidationFileAllowedExtensions);
                        }
                        if (productAttributeMapping.ValidationFileMaximumSize.HasValue)
                        {
                            xmlWriter.WriteElementString("ValidationFileMaximumSize", null, productAttributeMapping.ValidationFileMaximumSize.Value.ToString());
                        }
                        xmlWriter.WriteElementString("DefaultValue", null, productAttributeMapping.DefaultValue);
                    }
                    //conditions
                    xmlWriter.WriteElementString("ConditionAttributeXml", null, productAttributeMapping.ConditionAttributeXml);


                    xmlWriter.WriteStartElement("ProductAttributeValues");
                    var productAttributeValues = productAttributeMapping.ProductAttributeValues;
                    foreach (var productAttributeValue in productAttributeValues)
                    {
                        xmlWriter.WriteStartElement("ProductAttributeValue");
                        xmlWriter.WriteElementString("ProductAttributeValueId", null, productAttributeValue.Id.ToString());
                        xmlWriter.WriteElementString("Name", null, productAttributeValue.Name);
                        xmlWriter.WriteElementString("AttributeValueTypeId", null, productAttributeValue.AttributeValueTypeId.ToString());
                        xmlWriter.WriteElementString("AssociatedProductId", null, productAttributeValue.AssociatedProductId.ToString());
                        xmlWriter.WriteElementString("ColorSquaresRgb", null, productAttributeValue.ColorSquaresRgb);
                        xmlWriter.WriteElementString("ImageSquaresPictureId", null, productAttributeValue.ImageSquaresPictureId.ToString());
                        xmlWriter.WriteElementString("PriceAdjustment", null, productAttributeValue.PriceAdjustment.ToString());
                        xmlWriter.WriteElementString("WeightAdjustment", null, productAttributeValue.WeightAdjustment.ToString());
                        xmlWriter.WriteElementString("Cost", null, productAttributeValue.Cost.ToString());
                        xmlWriter.WriteElementString("Quantity", null, productAttributeValue.Quantity.ToString());
                        xmlWriter.WriteElementString("IsPreSelected", null, productAttributeValue.IsPreSelected.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productAttributeValue.DisplayOrder.ToString());
                        xmlWriter.WriteElementString("PictureId", null, productAttributeValue.PictureId.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();


                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();









                xmlWriter.WriteStartElement("ProductPictures");
                var productPictures = product.ProductPictures;
                foreach (var productPicture in productPictures)
                {
                    xmlWriter.WriteStartElement("ProductPicture");
                    xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
                    xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductCategories");
                var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                if (productCategories != null)
                {
                    foreach (var productCategory in productCategories)
                    {
                        xmlWriter.WriteStartElement("ProductCategory");
                        xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                        xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductManufacturers");
                var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
                if (productManufacturers != null)
                {
                    foreach (var productManufacturer in productManufacturers)
                    {
                        xmlWriter.WriteStartElement("ProductManufacturer");
                        xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                        xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductSpecificationAttributes");
                var productSpecificationAttributes = product.ProductSpecificationAttributes;
                foreach (var productSpecificationAttribute in productSpecificationAttributes)
                {
                    xmlWriter.WriteStartElement("ProductSpecificationAttribute");
                    xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
                    xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());
                    xmlWriter.WriteElementString("CustomValue", null, productSpecificationAttribute.CustomValue);
                    xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
                    xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="products">Products</param>
        public virtual byte[] ExportProductsToXlsx(IEnumerable<Product> products)
        {
            var properties = new[]
                {
                new PropertyByName<Product>("ProductType", p => p.ProductTypeId, IgnoreExportPoductProperty(p => p.ProductType))
                {
                    DropDownElements = ProductType.SimpleProduct.ToSelectList(useLocalization: false)
                },
                new PropertyByName<Product>("ParentGroupedProductId", p => p.ParentGroupedProductId, IgnoreExportPoductProperty(p => p.ProductType)),
                new PropertyByName<Product>("VisibleIndividually", p => p.VisibleIndividually, IgnoreExportPoductProperty(p => p.VisibleIndividually)),
                new PropertyByName<Product>("Name", p => p.Name),
                new PropertyByName<Product>("ShortDescription", p => p.ShortDescription),
                new PropertyByName<Product>("FullDescription", p => p.FullDescription),
                new PropertyByName<Product>("Vendor", p => p.VendorId, IgnoreExportPoductProperty(p => p.Vendor))
                {
                    DropDownElements = _vendorService.GetAllVendors(showHidden:true).Select(v => v as BaseEntity).ToSelectList(p => (p as Vendor).Return(v => v.Name, String.Empty)),
                    AllowBlank = true
                },
                new PropertyByName<Product>("ProductTemplate", p => p.ProductTemplateId, IgnoreExportPoductProperty(p => p.ProductTemplate))
                    {
                    DropDownElements = _productTemplateService.GetAllProductTemplates().Select(pt => pt as BaseEntity).ToSelectList(p => (p as ProductTemplate).Return(pt => pt.Name, String.Empty)),
                },
                new PropertyByName<Product>("ShowOnHomePage", p => p.ShowOnHomePage, IgnoreExportPoductProperty(p => p.ShowOnHomePage)),
                new PropertyByName<Product>("MetaKeywords", p => p.MetaKeywords, IgnoreExportPoductProperty(p => p.Seo)),
                new PropertyByName<Product>("MetaDescription", p => p.MetaDescription, IgnoreExportPoductProperty(p => p.Seo)),
                new PropertyByName<Product>("MetaTitle", p => p.MetaTitle, IgnoreExportPoductProperty(p => p.Seo)),
                new PropertyByName<Product>("SeName", p => p.GetSeName(0), IgnoreExportPoductProperty(p => p.Seo)),
                new PropertyByName<Product>("AllowCustomerReviews", p => p.AllowCustomerReviews, IgnoreExportPoductProperty(p => p.AllowCustomerReviews)),
                new PropertyByName<Product>("Published", p => p.Published, IgnoreExportPoductProperty(p => p.Published)),
                new PropertyByName<Product>("SKU", p => p.Sku),
                new PropertyByName<Product>("ManufacturerPartNumber", p => p.ManufacturerPartNumber, IgnoreExportPoductProperty(p => p.ManufacturerPartNumber)),
                new PropertyByName<Product>("Gtin", p => p.Gtin, IgnoreExportPoductProperty(p => p.GTIN)),
                new PropertyByName<Product>("IsGiftCard", p => p.IsGiftCard, IgnoreExportPoductProperty(p => p.IsGiftCard)),
                new PropertyByName<Product>("IsMealPlan", p => p.IsMealPlan, IgnoreExportPoductProperty(p => p.IsMealPlan)),	/// NU-16
                new PropertyByName<Product>("GiftCardType", p => p.GiftCardTypeId, IgnoreExportPoductProperty(p => p.IsGiftCard))
                    {
                    DropDownElements = GiftCardType.Virtual.ToSelectList(useLocalization: false)
                },
                new PropertyByName<Product>("OverriddenGiftCardAmount", p => p.OverriddenGiftCardAmount, IgnoreExportPoductProperty(p => p.IsGiftCard)),
                new PropertyByName<Product>("RequireOtherProducts", p => p.RequireOtherProducts, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart)),
                new PropertyByName<Product>("RequiredProductIds", p => p.RequiredProductIds, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart)),
                new PropertyByName<Product>("AutomaticallyAddRequiredProducts", p => p.AutomaticallyAddRequiredProducts, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart)),
                new PropertyByName<Product>("IsDownload", p => p.IsDownload, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("DownloadId", p => p.DownloadId, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("UnlimitedDownloads", p => p.UnlimitedDownloads, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("MaxNumberOfDownloads", p => p.MaxNumberOfDownloads, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("DownloadActivationType", p => p.DownloadActivationTypeId, IgnoreExportPoductProperty(p => p.DownloadableProduct))
                    {
                    DropDownElements = DownloadActivationType.Manually.ToSelectList(useLocalization: false)
                },
                new PropertyByName<Product>("HasSampleDownload", p => p.HasSampleDownload, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("SampleDownloadId", p => p.SampleDownloadId, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("HasUserAgreement", p => p.HasUserAgreement, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("UserAgreementText", p => p.UserAgreementText, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
                new PropertyByName<Product>("IsRecurring", p => p.IsRecurring, IgnoreExportPoductProperty(p => p.RecurringProduct)),
                new PropertyByName<Product>("RecurringCycleLength", p => p.RecurringCycleLength, IgnoreExportPoductProperty(p => p.RecurringProduct)),
                new PropertyByName<Product>("RecurringCyclePeriod", p => p.RecurringCyclePeriodId, IgnoreExportPoductProperty(p => p.RecurringProduct))
                        {
                    DropDownElements = RecurringProductCyclePeriod.Days.ToSelectList(useLocalization: false),
                    AllowBlank = true
                },
                new PropertyByName<Product>("RecurringTotalCycles", p => p.RecurringTotalCycles, IgnoreExportPoductProperty(p => p.RecurringProduct)),
                new PropertyByName<Product>("IsRental", p => p.IsRental, IgnoreExportPoductProperty(p => p.IsRental)),
                new PropertyByName<Product>("RentalPriceLength", p => p.RentalPriceLength, IgnoreExportPoductProperty(p => p.IsRental)),
                new PropertyByName<Product>("RentalPricePeriod", p => p.RentalPricePeriodId, IgnoreExportPoductProperty(p => p.IsRental))
                {
                    DropDownElements = RentalPricePeriod.Days.ToSelectList(useLocalization: false),
                    AllowBlank = true
                },
                new PropertyByName<Product>("IsShipEnabled", p => p.IsShipEnabled),
                new PropertyByName<Product>("IsFreeShipping", p => p.IsFreeShipping, IgnoreExportPoductProperty(p => p.FreeShipping)),
                new PropertyByName<Product>("ShipSeparately", p => p.ShipSeparately, IgnoreExportPoductProperty(p => p.ShipSeparately)),
                new PropertyByName<Product>("AdditionalShippingCharge", p => p.AdditionalShippingCharge, IgnoreExportPoductProperty(p => p.AdditionalShippingCharge)),
                new PropertyByName<Product>("DeliveryDate", p => p.DeliveryDateId, IgnoreExportPoductProperty(p => p.DeliveryDate))
                {
                    DropDownElements = _shippingService.GetAllDeliveryDates().Select(dd => dd as BaseEntity).ToSelectList(p => (p as DeliveryDate).Return(dd => dd.Name, String.Empty)),
                    AllowBlank = true
                },
                new PropertyByName<Product>("IsTaxExempt", p => p.IsTaxExempt),
                new PropertyByName<Product>("TaxCategory", p => p.TaxCategoryId)
                {
                    DropDownElements = _taxCategoryService.GetAllTaxCategories().Select(tc => tc as BaseEntity).ToSelectList(p => (p as TaxCategory).Return(tc => tc.Name, String.Empty)),
                    AllowBlank = true
                },
                new PropertyByName<Product>("IsTelecommunicationsOrBroadcastingOrElectronicServices", p => p.IsTelecommunicationsOrBroadcastingOrElectronicServices, IgnoreExportPoductProperty(p => p.TelecommunicationsBroadcastingElectronicServices)),
                new PropertyByName<Product>("ManageInventoryMethod", p => p.ManageInventoryMethodId)
                {
                    DropDownElements = ManageInventoryMethod.DontManageStock.ToSelectList(useLocalization: false)
                },
                new PropertyByName<Product>("UseMultipleWarehouses", p => p.UseMultipleWarehouses, IgnoreExportPoductProperty(p => p.UseMultipleWarehouses)),
                new PropertyByName<Product>("WarehouseId", p => p.WarehouseId, IgnoreExportPoductProperty(p => p.Warehouse)),
                new PropertyByName<Product>("StockQuantity", p => p.StockQuantity),
                new PropertyByName<Product>("DisplayStockAvailability", p => p.DisplayStockAvailability, IgnoreExportPoductProperty(p => p.DisplayStockAvailability)),
                new PropertyByName<Product>("DisplayStockQuantity", p => p.DisplayStockQuantity, IgnoreExportPoductProperty(p => p.DisplayStockQuantity)),
                new PropertyByName<Product>("MinStockQuantity", p => p.MinStockQuantity, IgnoreExportPoductProperty(p => p.MinimumStockQuantity)),
                new PropertyByName<Product>("LowStockActivity", p => p.LowStockActivityId, IgnoreExportPoductProperty(p => p.LowStockActivity))
                {
                    DropDownElements = LowStockActivity.Nothing.ToSelectList(useLocalization: false)
                },
                new PropertyByName<Product>("NotifyAdminForQuantityBelow", p => p.NotifyAdminForQuantityBelow, IgnoreExportPoductProperty(p => p.NotifyAdminForQuantityBelow)),
                new PropertyByName<Product>("BackorderMode", p => p.BackorderModeId, IgnoreExportPoductProperty(p => p.Backorders))
                {
                    DropDownElements = BackorderMode.NoBackorders.ToSelectList(useLocalization: false)
                },
                new PropertyByName<Product>("AllowBackInStockSubscriptions", p => p.AllowBackInStockSubscriptions, IgnoreExportPoductProperty(p => p.AllowBackInStockSubscriptions)),
                new PropertyByName<Product>("OrderMinimumQuantity", p => p.OrderMinimumQuantity, IgnoreExportPoductProperty(p => p.MinimumCartQuantity)),
                new PropertyByName<Product>("OrderMaximumQuantity", p => p.OrderMaximumQuantity, IgnoreExportPoductProperty(p => p.MaximumCartQuantity)),
                new PropertyByName<Product>("AllowedQuantities", p => p.AllowedQuantities, IgnoreExportPoductProperty(p => p.AllowedQuantities)),
                new PropertyByName<Product>("AllowAddingOnlyExistingAttributeCombinations", p => p.AllowAddingOnlyExistingAttributeCombinations, IgnoreExportPoductProperty(p => p.AllowAddingOnlyExistingAttributeCombinations)),
                new PropertyByName<Product>("NotReturnable", p => p.NotReturnable, IgnoreExportPoductProperty(p => p.NotReturnable)),
                new PropertyByName<Product>("DisableBuyButton", p => p.DisableBuyButton, IgnoreExportPoductProperty(p => p.DisableBuyButton)),
                new PropertyByName<Product>("DisableWishlistButton", p => p.DisableWishlistButton, IgnoreExportPoductProperty(p => p.DisableWishlistButton)),
                new PropertyByName<Product>("AvailableForPreOrder", p => p.AvailableForPreOrder, IgnoreExportPoductProperty(p => p.AvailableForPreOrder)),
                new PropertyByName<Product>("PreOrderAvailabilityStartDateTimeUtc", p => p.PreOrderAvailabilityStartDateTimeUtc, IgnoreExportPoductProperty(p => p.AvailableForPreOrder)),
                new PropertyByName<Product>("CallForPrice", p => p.CallForPrice, IgnoreExportPoductProperty(p => p.CallForPrice)),
                new PropertyByName<Product>("Price", p => p.Price),
                new PropertyByName<Product>("OldPrice", p => p.OldPrice, IgnoreExportPoductProperty(p => p.OldPrice)),
                new PropertyByName<Product>("ProductCost", p => p.ProductCost, IgnoreExportPoductProperty(p => p.ProductCost)),
                new PropertyByName<Product>("SpecialPrice", p => p.SpecialPrice, IgnoreExportPoductProperty(p => p.SpecialPrice)),
                new PropertyByName<Product>("SpecialPriceStartDateTimeUtc", p => p.SpecialPriceStartDateTimeUtc, IgnoreExportPoductProperty(p => p.SpecialPriceStartDate)),
                new PropertyByName<Product>("SpecialPriceEndDateTimeUtc", p => p.SpecialPriceEndDateTimeUtc, IgnoreExportPoductProperty(p => p.SpecialPriceEndDate)),
                new PropertyByName<Product>("CustomerEntersPrice", p => p.CustomerEntersPrice, IgnoreExportPoductProperty(p => p.CustomerEntersPrice)),
                new PropertyByName<Product>("MinimumCustomerEnteredPrice", p => p.MinimumCustomerEnteredPrice, IgnoreExportPoductProperty(p => p.CustomerEntersPrice)),
                new PropertyByName<Product>("MaximumCustomerEnteredPrice", p => p.MaximumCustomerEnteredPrice, IgnoreExportPoductProperty(p => p.CustomerEntersPrice)),
                new PropertyByName<Product>("BasepriceEnabled", p => p.BasepriceEnabled, IgnoreExportPoductProperty(p => p.PAngV)),
                new PropertyByName<Product>("BasepriceAmount", p => p.BasepriceAmount, IgnoreExportPoductProperty(p => p.PAngV)),
                new PropertyByName<Product>("BasepriceUnit", p => p.BasepriceUnitId, IgnoreExportPoductProperty(p => p.PAngV))
                {
                    DropDownElements = _measureService.GetAllMeasureWeights().Select(mw => mw as BaseEntity).ToSelectList(p => (p as MeasureWeight).Return(mw => mw.Name, String.Empty)),
                    AllowBlank = true
                },
                new PropertyByName<Product>("BasepriceBaseAmount", p => p.BasepriceBaseAmount, IgnoreExportPoductProperty(p => p.PAngV)),
                new PropertyByName<Product>("BasepriceBaseUnit", p => p.BasepriceBaseUnitId, IgnoreExportPoductProperty(p => p.PAngV))
                {
                    DropDownElements = _measureService.GetAllMeasureWeights().Select(mw => mw as BaseEntity).ToSelectList(p => (p as MeasureWeight).Return(mw => mw.Name, String.Empty)),
                    AllowBlank = true
                },
                new PropertyByName<Product>("MarkAsNew", p => p.MarkAsNew, IgnoreExportPoductProperty(p => p.MarkAsNew)),
                new PropertyByName<Product>("MarkAsNewStartDateTimeUtc", p => p.MarkAsNewStartDateTimeUtc, IgnoreExportPoductProperty(p => p.MarkAsNewStartDate)),
                new PropertyByName<Product>("MarkAsNewEndDateTimeUtc", p => p.MarkAsNewEndDateTimeUtc, IgnoreExportPoductProperty(p => p.MarkAsNewEndDate)),
                new PropertyByName<Product>("Weight", p => p.Weight, IgnoreExportPoductProperty(p => p.Weight)),
                new PropertyByName<Product>("Length", p => p.Length, IgnoreExportPoductProperty(p => p.Dimensions)),
                new PropertyByName<Product>("Width", p => p.Width, IgnoreExportPoductProperty(p => p.Dimensions)),
                new PropertyByName<Product>("Height", p => p.Height, IgnoreExportPoductProperty(p => p.Dimensions)),
                new PropertyByName<Product>("Categories", GetCategories),
                new PropertyByName<Product>("Manufacturers", GetManufacturers, IgnoreExportPoductProperty(p => p.Manufacturers)),
                new PropertyByName<Product>("Picture1", p => GetPictures(p)[0]),
                new PropertyByName<Product>("Picture2", p => GetPictures(p)[1]),
                new PropertyByName<Product>("Picture3", p => GetPictures(p)[2])
            };

            var productList = products.ToList();
            var productAdvancedMode = _workContext.CurrentCustomer.GetAttribute<bool>("product-advanced-mode");

            if (_catalogSettings.ExportImportProductAttributes)
            {
                if (productAdvancedMode || _productEditorSettings.ProductAttributes)
                    return ExportProductsToXlsxWithAttributes(properties, productList);
            }

            return ExportToXlsx(properties, productList);
        }

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportOrdersToXml(IList<Order> orders)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Orders");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);


            foreach (var order in orders)
            {
                xmlWriter.WriteStartElement("Order");

                xmlWriter.WriteElementString("OrderId", null, order.Id.ToString());
                xmlWriter.WriteElementString("OrderGuid", null, order.OrderGuid.ToString());
                xmlWriter.WriteElementString("StoreId", null, order.StoreId.ToString());
                xmlWriter.WriteElementString("CustomerId", null, order.CustomerId.ToString());
                xmlWriter.WriteElementString("OrderStatusId", null, order.OrderStatusId.ToString());
                xmlWriter.WriteElementString("PaymentStatusId", null, order.PaymentStatusId.ToString());
                xmlWriter.WriteElementString("ShippingStatusId", null, order.ShippingStatusId.ToString());
                xmlWriter.WriteElementString("CustomerLanguageId", null, order.CustomerLanguageId.ToString());
                xmlWriter.WriteElementString("CustomerTaxDisplayTypeId", null, order.CustomerTaxDisplayTypeId.ToString());
                xmlWriter.WriteElementString("CustomerIp", null, order.CustomerIp);
                xmlWriter.WriteElementString("OrderSubtotalInclTax", null, order.OrderSubtotalInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubtotalExclTax", null, order.OrderSubtotalExclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountInclTax", null, order.OrderSubTotalDiscountInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountExclTax", null, order.OrderSubTotalDiscountExclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingInclTax", null, order.OrderShippingInclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingExclTax", null, order.OrderShippingExclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeInclTax", null, order.PaymentMethodAdditionalFeeInclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeExclTax", null, order.PaymentMethodAdditionalFeeExclTax.ToString());
                xmlWriter.WriteElementString("TaxRates", null, order.TaxRates);
                xmlWriter.WriteElementString("OrderTax", null, order.OrderTax.ToString());
                xmlWriter.WriteElementString("OrderTotal", null, order.OrderTotal.ToString());
                xmlWriter.WriteElementString("RefundedAmount", null, order.RefundedAmount.ToString());
                xmlWriter.WriteElementString("OrderDiscount", null, order.OrderDiscount.ToString());
                xmlWriter.WriteElementString("CurrencyRate", null, order.CurrencyRate.ToString());
                xmlWriter.WriteElementString("CustomerCurrencyCode", null, order.CustomerCurrencyCode);
                xmlWriter.WriteElementString("AffiliateId", null, order.AffiliateId.ToString());
                xmlWriter.WriteElementString("AllowStoringCreditCardNumber", null, order.AllowStoringCreditCardNumber.ToString());
                xmlWriter.WriteElementString("CardType", null, order.CardType);
                xmlWriter.WriteElementString("CardName", null, order.CardName);
                xmlWriter.WriteElementString("CardNumber", null, order.CardNumber);
                xmlWriter.WriteElementString("MaskedCreditCardNumber", null, order.MaskedCreditCardNumber);
                xmlWriter.WriteElementString("CardCvv2", null, order.CardCvv2);
                xmlWriter.WriteElementString("CardExpirationMonth", null, order.CardExpirationMonth);
                xmlWriter.WriteElementString("CardExpirationYear", null, order.CardExpirationYear);
                xmlWriter.WriteElementString("PaymentMethodSystemName", null, order.PaymentMethodSystemName);
                xmlWriter.WriteElementString("AuthorizationTransactionId", null, order.AuthorizationTransactionId);
                xmlWriter.WriteElementString("AuthorizationTransactionCode", null, order.AuthorizationTransactionCode);
                xmlWriter.WriteElementString("AuthorizationTransactionResult", null, order.AuthorizationTransactionResult);
                xmlWriter.WriteElementString("CaptureTransactionId", null, order.CaptureTransactionId);
                xmlWriter.WriteElementString("CaptureTransactionResult", null, order.CaptureTransactionResult);
                xmlWriter.WriteElementString("SubscriptionTransactionId", null, order.SubscriptionTransactionId);
                xmlWriter.WriteElementString("PaidDateUtc", null, (order.PaidDateUtc == null) ? string.Empty : order.PaidDateUtc.Value.ToString());
                xmlWriter.WriteElementString("ShippingMethod", null, order.ShippingMethod);
                xmlWriter.WriteElementString("ShippingRateComputationMethodSystemName", null, order.ShippingRateComputationMethodSystemName);
                xmlWriter.WriteElementString("CustomValuesXml", null, order.CustomValuesXml);
                xmlWriter.WriteElementString("VatNumber", null, order.VatNumber);
                xmlWriter.WriteElementString("Deleted", null, order.Deleted.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, order.CreatedOnUtc.ToString());

                //products
                var orderItems = order.OrderItems;
                if (orderItems.Any())
                {
                    xmlWriter.WriteStartElement("OrderItems");
                    foreach (var orderItem in orderItems)
                    {
                        xmlWriter.WriteStartElement("OrderItem");
                        xmlWriter.WriteElementString("Id", null, orderItem.Id.ToString());
                        xmlWriter.WriteElementString("OrderItemGuid", null, orderItem.OrderItemGuid.ToString());
                        xmlWriter.WriteElementString("ProductId", null, orderItem.ProductId.ToString());

                        var product = orderItem.Product;
                        xmlWriter.WriteElementString("ProductName", null, product.Name);
                        xmlWriter.WriteElementString("UnitPriceInclTax", null, orderItem.UnitPriceInclTax.ToString());
                        xmlWriter.WriteElementString("UnitPriceExclTax", null, orderItem.UnitPriceExclTax.ToString());
                        xmlWriter.WriteElementString("PriceInclTax", null, orderItem.PriceInclTax.ToString());
                        xmlWriter.WriteElementString("PriceExclTax", null, orderItem.PriceExclTax.ToString());
                        xmlWriter.WriteElementString("DiscountAmountInclTax", null, orderItem.DiscountAmountInclTax.ToString());
                        xmlWriter.WriteElementString("DiscountAmountExclTax", null, orderItem.DiscountAmountExclTax.ToString());
                        xmlWriter.WriteElementString("OriginalProductCost", null, orderItem.OriginalProductCost.ToString());
                        xmlWriter.WriteElementString("AttributeDescription", null, orderItem.AttributeDescription);
                        xmlWriter.WriteElementString("AttributesXml", null, orderItem.AttributesXml);
                        xmlWriter.WriteElementString("Quantity", null, orderItem.Quantity.ToString());
                        xmlWriter.WriteElementString("DownloadCount", null, orderItem.DownloadCount.ToString());
                        xmlWriter.WriteElementString("IsDownloadActivated", null, orderItem.IsDownloadActivated.ToString());
                        xmlWriter.WriteElementString("LicenseDownloadId", null, orderItem.LicenseDownloadId.ToString());
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : String.Empty;
                        xmlWriter.WriteElementString("RentalStartDateUtc", null, rentalStartDate);
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : String.Empty;
                        xmlWriter.WriteElementString("RentalEndDateUtc", null, rentalEndDate);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }

                //shipments
                var shipments = order.Shipments.OrderBy(x => x.CreatedOnUtc).ToList();
                if (shipments.Any())
                {
                    xmlWriter.WriteStartElement("Shipments");
                    foreach (var shipment in shipments)
                    {
                        xmlWriter.WriteStartElement("Shipment");
                        xmlWriter.WriteElementString("ShipmentId", null, shipment.Id.ToString());
                        xmlWriter.WriteElementString("TrackingNumber", null, shipment.TrackingNumber);
                        xmlWriter.WriteElementString("TotalWeight", null, shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value.ToString() : String.Empty);

                        xmlWriter.WriteElementString("ShippedDateUtc", null, shipment.ShippedDateUtc.HasValue ?
                            shipment.ShippedDateUtc.ToString() : String.Empty);
                        xmlWriter.WriteElementString("DeliveryDateUtc", null, shipment.DeliveryDateUtc.HasValue ?
                            shipment.DeliveryDateUtc.Value.ToString() : String.Empty);
                        xmlWriter.WriteElementString("CreatedOnUtc", null, shipment.CreatedOnUtc.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        public virtual byte[] ExportOrdersToXlsx(IList<Order> orders)
        {
            //property array
            var properties = new[]
                    {
                    new PropertyByName<Order>("OrderId", p => p.Id),
                    new PropertyByName<Order>("StoreId", p => p.StoreId),
                    new PropertyByName<Order>("OrderGuid", p => p.OrderGuid),
                    new PropertyByName<Order>("CustomerId", p => p.CustomerId),
                    new PropertyByName<Order>("CustomerName", p=>p.Customer.Username),
                    new PropertyByName<Order>("Email", p => p.Customer.Email),
                    new PropertyByName<Order>("OrderStatusId", p => p.OrderStatusId),
                    new PropertyByName<Order>("PaymentStatusId", p => p.PaymentStatusId),
                    new PropertyByName<Order>("ShippingStatusId", p => p.ShippingStatusId),
                    new PropertyByName<Order>("OrderSubtotalInclTax", p => p.OrderSubtotalInclTax),
                    new PropertyByName<Order>("OrderSubtotalExclTax", p => p.OrderSubtotalExclTax),
                    new PropertyByName<Order>("OrderSubTotalDiscountInclTax", p => p.OrderSubTotalDiscountInclTax),
                    new PropertyByName<Order>("OrderSubTotalDiscountExclTax", p => p.OrderSubTotalDiscountExclTax),
                    new PropertyByName<Order>("OrderShippingInclTax", p => p.OrderShippingInclTax),
                    new PropertyByName<Order>("OrderShippingExclTax", p => p.OrderShippingExclTax),
                    new PropertyByName<Order>("PaymentMethodAdditionalFeeInclTax", p => p.PaymentMethodAdditionalFeeInclTax),
                    new PropertyByName<Order>("PaymentMethodAdditionalFeeExclTax", p => p.PaymentMethodAdditionalFeeExclTax),
                    new PropertyByName<Order>("TaxRates", p => p.TaxRates),
                    new PropertyByName<Order>("OrderTax", p => p.OrderTax),
                    new PropertyByName<Order>("OrderTotal", p => p.OrderTotal),
                    new PropertyByName<Order>("RefundedAmount", p => p.RefundedAmount),
                    new PropertyByName<Order>("OrderDiscount", p => p.OrderDiscount),
                    new PropertyByName<Order>("CurrencyRate", p => p.CurrencyRate),
                    new PropertyByName<Order>("CustomerCurrencyCode", p => p.CustomerCurrencyCode),
                    new PropertyByName<Order>("AffiliateId", p => p.AffiliateId),
                    new PropertyByName<Order>("PaymentMethodSystemName", p => p.PaymentMethodSystemName),
                    new PropertyByName<Order>("ShippingPickUpInStore", p => p.PickUpInStore),
                    new PropertyByName<Order>("ShippingMethod", p => p.ShippingMethod),
                    new PropertyByName<Order>("ShippingRateComputationMethodSystemName", p => p.ShippingRateComputationMethodSystemName),
                    new PropertyByName<Order>("CustomValuesXml", p => p.CustomValuesXml),
                    new PropertyByName<Order>("VatNumber", p => p.VatNumber),
                    new PropertyByName<Order>("CreatedOnUtc", p => p.CreatedOnUtc.ToOADate()),
                    new PropertyByName<Order>("BillingFirstName", p => p.BillingAddress.Return(billingAddress => billingAddress.FirstName, String.Empty)),
                    new PropertyByName<Order>("BillingLastName", p => p.BillingAddress.Return(billingAddress => billingAddress.LastName, String.Empty)),
                    new PropertyByName<Order>("BillingEmail", p => p.BillingAddress.Return(billingAddress => billingAddress.Email, String.Empty)),
                    new PropertyByName<Order>("BillingCompany", p => p.BillingAddress.Return(billingAddress => billingAddress.Company, String.Empty)),
                    new PropertyByName<Order>("BillingCountry", p => p.BillingAddress.Return(billingAddress => billingAddress.Country, null).Return(country => country.Name, String.Empty)),
                    new PropertyByName<Order>("BillingStateProvince", p => p.BillingAddress.Return(billingAddress => billingAddress.StateProvince, null).Return(stateProvince => stateProvince.Name, String.Empty)),
                    new PropertyByName<Order>("BillingCity", p => p.BillingAddress.Return(billingAddress => billingAddress.City, String.Empty)),
                    new PropertyByName<Order>("BillingAddress1", p => p.BillingAddress.Return(billingAddress => billingAddress.Address1, String.Empty)),
                    new PropertyByName<Order>("BillingAddress2", p => p.BillingAddress.Return(billingAddress => billingAddress.Address2, String.Empty)),
                    new PropertyByName<Order>("BillingZipPostalCode", p => p.BillingAddress.Return(billingAddress => billingAddress.ZipPostalCode, String.Empty)),
                    new PropertyByName<Order>("BillingPhoneNumber", p => p.BillingAddress.Return(billingAddress => billingAddress.PhoneNumber, String.Empty)),
                    new PropertyByName<Order>("BillingFaxNumber", p => p.BillingAddress.Return(billingAddress => billingAddress.FaxNumber, String.Empty)),
                    new PropertyByName<Order>("ShippingFirstName", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.FirstName, String.Empty)),
                    new PropertyByName<Order>("ShippingLastName", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.LastName, String.Empty)),
                    new PropertyByName<Order>("ShippingEmail", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Email, String.Empty)),
                    new PropertyByName<Order>("ShippingCompany", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Company, String.Empty)),
                    new PropertyByName<Order>("ShippingCountry", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Country, null).Return(country => country.Name, String.Empty)),
                    new PropertyByName<Order>("ShippingStateProvince", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.StateProvince, null).Return(stateProvince => stateProvince.Name, String.Empty)),
                    new PropertyByName<Order>("ShippingCity", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.City, String.Empty)),
                    new PropertyByName<Order>("ShippingAddress1", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Address1, String.Empty)),
                    new PropertyByName<Order>("ShippingAddress2", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Address2, String.Empty)),
                    new PropertyByName<Order>("ShippingZipPostalCode", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.ZipPostalCode, String.Empty)),
                    new PropertyByName<Order>("ShippingPhoneNumber", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.PhoneNumber, String.Empty)),
                    new PropertyByName<Order>("ShippingFaxNumber", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.FaxNumber, String.Empty))
                    };

            return ExportToXlsx(properties, orders);
        }

        public virtual byte[] ExportKitchenProdutionOrdersToXlsx(IList<KitchenProduction> kitchenProduction)
        {
            var properties = new[]
            {
                    new PropertyByName<KitchenProduction>("Order Date", p => p.createdOnDate),
                    new PropertyByName<KitchenProduction>("OrderId", p => p.orderid),
                    new PropertyByName<KitchenProduction>("Product Name", p => p.productName),
                    new PropertyByName<KitchenProduction>("Attributes", p => p.attributes),
                    new PropertyByName<KitchenProduction>("Pickup Date", p => p.pickupDate),
                    new PropertyByName<KitchenProduction>("Pickup Location", p => p.pickupLocation),
                    new PropertyByName<KitchenProduction>("Customer Name", p => p.userName),
                    new PropertyByName<KitchenProduction>("Email", p => p.email),

            };
            return ExportKitchenProdutionToXlsx(properties, kitchenProduction); ;
        }

        /// <summary>
        /// Excel report to pull the Kitchen Production Data
        /// </summary>
        /// <param name="shipmentReports"></param>
        /// <returns></returns>
        public virtual byte[] ExportShipmentReportToXlsx(IList<ShipmentReport> shipmentReports)
        {
            //property array
            var properties = new[]
                    {
                    new PropertyByName<ShipmentReport>("OrderId", p => p.OrderId),
                    new PropertyByName<ShipmentReport>("StoreId", p => p.StoreId),
                    new PropertyByName<ShipmentReport>("OrderStatus", p => p.OrderStatus),
                    new PropertyByName<ShipmentReport>("OrderStatus", p => p.ShippingStatus),
                    new PropertyByName<ShipmentReport>("OrderTotal", p => p.OrderTotal),
                    new PropertyByName<ShipmentReport>("ShipmentID", p => p.ShipmentID),
                    new PropertyByName<ShipmentReport>("ShippingMethod", p => p.ShippingMethod),
                    new PropertyByName<ShipmentReport>("ShipmentTrackingNumber", p => p.ShipmentTrackingNumber),
                    new PropertyByName<ShipmentReport>("ShippingCost", p => p.ShippingCost),
                    new PropertyByName<ShipmentReport>("RecipientContactInfo", p => p.RecipientContactInfo),
                    new PropertyByName<ShipmentReport>("RecipientName", p => p.RecipientName),
                    new PropertyByName<ShipmentReport>("RecipientCountry", p => p.RecipientCountry),
                    new PropertyByName<ShipmentReport>("RecipientStateProvince", p => p.RecipientStateProvince),
                    new PropertyByName<ShipmentReport>("RecipientCity", p => p.RecipientCity),
                    new PropertyByName<ShipmentReport>("RecipientAddress1", p => p.RecipientAddress1),
                    new PropertyByName<ShipmentReport>("RecipientAddress2", p => p.RecipientAddress2),
                    new PropertyByName<ShipmentReport>("RecipientZipPostalCode", p => p.RecipientZipPostalCode)
                    };

            return ExportToXlsx(properties, shipmentReports);
        }
        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        /// <summary>
        /// Excel report to pull the Reservation Reports
        /// </summary>
        /// <param name="reservationReports"></param>
        /// <returns></returns>
        public virtual byte[] ReservationReportsToXlsx(IList<ReservationGridListModel> reservationReports)
        {
            //property array
            var properties = new[]
                    {
                        new PropertyByName<ReservationGridListModel>("OrderId", p => p.OrderId),
                        //new PropertyByName<ReservationGridListModel>("ReservedProductId", p => p.ReservedProductId),
                        new PropertyByName<ReservationGridListModel>("ReservationDate", p => p.ReservationDate),
                        new PropertyByName<ReservationGridListModel>("ReservedTimeSlot", p => p.ReservedTimeSlot),
                        new PropertyByName<ReservationGridListModel>("ReservedUnits", p => p.ReservedUnits),
                        //new PropertyByName<ReservationGridListModel>("OrderItemId", p => p.OrderItemId),
                        //new PropertyByName<ReservationGridListModel>("PaymentStatusId", p => p.PaymentStatusId),
                        //new PropertyByName<ReservationGridListModel>("CustomerId", p => p.CustomerId),
                        new PropertyByName<ReservationGridListModel>("CustomerFullName", p => p.CustomerFullName),
                        new PropertyByName<ReservationGridListModel>("CustomerEmail", p => p.CustomerEmail),
                        new PropertyByName<ReservationGridListModel>("OrderTotal", p => p.OrderTotal),
                        new PropertyByName<ReservationGridListModel>("PaymentStatus", p => p.PaymentStatus),
                        //new PropertyByName<ReservationGridListModel>("IsFulfilled", p => p.IsFulfilled),
                        new PropertyByName<ReservationGridListModel>("ProductId", p => p.ProductId),
                        new PropertyByName<ReservationGridListModel>("ProductName", p => p.ProductName)
                     };

            return ExportToXlsx(properties, reservationReports);
        }

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="customers">Customers</param>
        public virtual byte[] ExportCustomersToXlsx(IList<Customer> customers)
        {
            //property array
            var properties = new[]
                    {
                new PropertyByName<Customer>("CustomerId", p => p.Id),
                new PropertyByName<Customer>("CustomerGuid", p => p.CustomerGuid),
                new PropertyByName<Customer>("Email", p => p.Email),
                new PropertyByName<Customer>("Username", p => p.Username),
                new PropertyByName<Customer>("Password", p => p.Password),
                new PropertyByName<Customer>("PasswordFormatId", p => p.PasswordFormatId),
                new PropertyByName<Customer>("PasswordSalt", p => p.PasswordSalt),
                new PropertyByName<Customer>("IsTaxExempt", p => p.IsTaxExempt),
                new PropertyByName<Customer>("AffiliateId", p => p.AffiliateId),
                new PropertyByName<Customer>("VendorId", p => p.VendorId),
                new PropertyByName<Customer>("Active", p => p.Active),
                new PropertyByName<Customer>("IsGuest", p => p.IsGuest()),
                new PropertyByName<Customer>("IsRegistered", p => p.IsRegistered()),
                new PropertyByName<Customer>("IsAdministrator", p => p.IsAdmin()),
                new PropertyByName<Customer>("IsForumModerator", p => p.IsForumModerator()),
                    //attributes
                new PropertyByName<Customer>("FirstName", p => p.GetAttribute<string>(SystemCustomerAttributeNames.FirstName)),
                new PropertyByName<Customer>("LastName", p => p.GetAttribute<string>(SystemCustomerAttributeNames.LastName)),
                new PropertyByName<Customer>("Gender", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Gender)),
                new PropertyByName<Customer>("Company", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Company)),
                new PropertyByName<Customer>("StreetAddress", p => p.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress)),
                new PropertyByName<Customer>("StreetAddress2", p => p.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2)),
                new PropertyByName<Customer>("ZipPostalCode", p => p.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode)),
                new PropertyByName<Customer>("City", p => p.GetAttribute<string>(SystemCustomerAttributeNames.City)),
                new PropertyByName<Customer>("CountryId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.CountryId)),
                new PropertyByName<Customer>("StateProvinceId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId)),
                new PropertyByName<Customer>("Phone", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Phone)),
                new PropertyByName<Customer>("Fax", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Fax)),
                new PropertyByName<Customer>("VatNumber", p => p.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber)),
                new PropertyByName<Customer>("VatNumberStatusId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId)),
                new PropertyByName<Customer>("TimeZoneId", p => p.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId)),
                new PropertyByName<Customer>("AvatarPictureId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId)),
                new PropertyByName<Customer>("ForumPostCount", p => p.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount)),
                new PropertyByName<Customer>("Signature", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Signature)),
            };

            return ExportToXlsx(properties, customers);
        }

        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCustomersToXml(IList<Customer> customers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Customers");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var customer in customers)
            {
                xmlWriter.WriteStartElement("Customer");
                xmlWriter.WriteElementString("CustomerId", null, customer.Id.ToString());
                xmlWriter.WriteElementString("CustomerGuid", null, customer.CustomerGuid.ToString());
                xmlWriter.WriteElementString("Email", null, customer.Email);
                xmlWriter.WriteElementString("Username", null, customer.Username);
                xmlWriter.WriteElementString("Password", null, customer.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, customer.PasswordFormatId.ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, customer.PasswordSalt);
                xmlWriter.WriteElementString("IsTaxExempt", null, customer.IsTaxExempt.ToString());
                xmlWriter.WriteElementString("AffiliateId", null, customer.AffiliateId.ToString());
                xmlWriter.WriteElementString("VendorId", null, customer.VendorId.ToString());
                xmlWriter.WriteElementString("Active", null, customer.Active.ToString());


                xmlWriter.WriteElementString("IsGuest", null, customer.IsGuest().ToString());
                xmlWriter.WriteElementString("IsRegistered", null, customer.IsRegistered().ToString());
                xmlWriter.WriteElementString("IsAdministrator", null, customer.IsAdmin().ToString());
                xmlWriter.WriteElementString("IsForumModerator", null, customer.IsForumModerator().ToString());

                xmlWriter.WriteElementString("FirstName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName));
                xmlWriter.WriteElementString("LastName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName));
                xmlWriter.WriteElementString("Gender", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender));
                xmlWriter.WriteElementString("Company", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Company));

                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StreetAddress", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress));
                xmlWriter.WriteElementString("StreetAddress2", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2));
                xmlWriter.WriteElementString("ZipPostalCode", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode));
                xmlWriter.WriteElementString("City", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.City));
                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());	// SODMYWAY-
                xmlWriter.WriteElementString("StateProvinceId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId).ToString());
                xmlWriter.WriteElementString("Phone", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone));
                xmlWriter.WriteElementString("Fax", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax));
                xmlWriter.WriteElementString("VatNumber", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber));
                xmlWriter.WriteElementString("VatNumberStatusId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId).ToString());
                xmlWriter.WriteElementString("TimeZoneId", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId));

                foreach (var store in _storeService.GetAllStores())
                {
                    var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    bool subscribedToNewsletters = newsletter != null && newsletter.Active;
                    xmlWriter.WriteElementString(string.Format("Newsletter-in-store-{0}", store.Id), null, subscribedToNewsletters.ToString());
                }

                xmlWriter.WriteElementString("AvatarPictureId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId).ToString());
                xmlWriter.WriteElementString("ForumPostCount", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount).ToString());
                xmlWriter.WriteElementString("Signature", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature));

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        #region NU-32
        /// <summary>
        /// Export meal plans to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="mealplans">Orders</param>
        public virtual byte[] ExportMealPlansToXlsx(IList<MealPlan> mealplans, bool exportGrouped)
        {
            //property array
            var properties = new[]{
                new PropertyByName<MealPlan>("Id", p => p.Id),
                new PropertyByName<MealPlan>("Purchase Date", p => p.CreatedOnUtc.ToOADate()),
                new PropertyByName<MealPlan>("Meal Plan", p => p.PurchasedWithOrderItem.Product.Name),
                new PropertyByName<MealPlan>("Account No.", p => p.RecipientAcctNum),
                new PropertyByName<MealPlan>("Amount", p=> { return (exportGrouped == true) ? p.PurchasedWithOrderItem.PriceExclTax:p.PurchasedWithOrderItem.UnitPriceExclTax; }),
                //new PropertyByName<MealPlan>("Amount", p => p.PurchasedWithOrderItem.PriceExclTax),
                new PropertyByName<MealPlan>("Recipient Name", p => p.RecipientName),
                new PropertyByName<MealPlan>("Recipient Address", p => p.RecipientAddress),
                new PropertyByName<MealPlan>("Recipient Phone", p => p.RecipientPhone),
                new PropertyByName<MealPlan>("Recipient Email", p => p.RecipientEmail),
            };

            return ExportToXlsx(properties, mealplans);
        }

        static void SetTitleStyle(ExcelRange cell)
        {
            cell.Style.Font.Size = 14;
        }

        static void SetHeaderStyle(ExcelRange cell)
        {
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
            cell.Style.Font.Bold = true;
        }
        #endregion

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions)
        {
            if (subscriptions == null)
                throw new ArgumentNullException("subscriptions");

            const string separator = ",";
            var sb = new StringBuilder();
            foreach (var subscription in subscriptions)
            {
                sb.Append(subscription.Email);
                sb.Append(separator);
                sb.Append(subscription.Active);
                sb.Append(separator);
                sb.Append(subscription.StoreId);
                sb.Append(Environment.NewLine);  //new line
            }
            return sb.ToString();
        }

        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportStatesToTxt(IList<StateProvince> states)
        {
            if (states == null)
                throw new ArgumentNullException("states");

            const string separator = ",";
            var sb = new StringBuilder();
            foreach (var state in states)
            {
                sb.Append(state.Country.TwoLetterIsoCode);
                sb.Append(separator);
                sb.Append(state.Name);
                sb.Append(separator);
                sb.Append(state.Abbreviation);
                sb.Append(separator);
                sb.Append(state.Published);
                sb.Append(separator);
                sb.Append(state.DisplayOrder);
                sb.Append(Environment.NewLine);  //new line
            }
            return sb.ToString();
        }

        //public virtual byte[] ExportMealPlansToXlsx(IList<MealPlan> mealplans, bool exportGrouped)
        //{

        //    var properties = new[]{
        //        new PropertyByName<MealPlan>("Id", p => p.Id),
        //        new PropertyByName<MealPlan>("Purchase Date", p => p.CreatedOnUtc.ToOADate()),
        //        new PropertyByName<MealPlan>("Meal Plan", p => p.PurchasedWithOrderItem.Product.Name),
        //        new PropertyByName<MealPlan>("Account No.", p => p.RecipientAcctNum),
        //        new PropertyByName<MealPlan>("Amount", p=> { return (exportGrouped == true) ? p.PurchasedWithOrderItem.PriceInclTax:p.PurchasedWithOrderItem.UnitPriceInclTax; }),
        //        new PropertyByName<MealPlan>("Recipient Name", p => p.RecipientName),
        //        new PropertyByName<MealPlan>("Recipient Address", p => p.RecipientAddress),
        //        new PropertyByName<MealPlan>("Recipient Phone", p => p.RecipientPhone),
        //        new PropertyByName<MealPlan>("Recipient Email", p => p.RecipientEmail),
        //        };

        //    return ExportToXlsx(properties, mealplans);
        //}


        #region NU-33

        /// <summary>
        /// Exports donations to Excel
        /// </summary>
        /// <param name="donations"></param>
        /// <param name="filterData"></param>
        /// <returns></returns>
        public virtual byte[] ExportDonationsToXlsx(IList<Donation> donations, bool filterData)
        {
            //property array
            if (filterData)
            {
                var properties = new[]
                {

                new PropertyByName<Donation>("DonorFirstName", p => p.DonorFirstName),
                new PropertyByName<Donation>("DonorLastName", p => p.DonorLastName),
                new PropertyByName<Donation>("DonorCompany", p => p.DonorCompany),
                new PropertyByName<Donation>("DonorAddress", p => p.DonorAddress),
                new PropertyByName<Donation>("DonorAddress2", p => p.DonorAddress2),
                new PropertyByName<Donation>("DonorCity", p => p.DonorCity),
                new PropertyByName<Donation>("DonorState", p => p.DonorState),
                new PropertyByName<Donation>("DonorZip", p => p.DonorZip),
                new PropertyByName<Donation>("DonorPhone", p => p.DonorPhone),
                new PropertyByName<Donation>("DonorCountry", p => p.DonorCountry),
                };
                return ExportToXlsx(properties, donations);

            }

            else
            {
                var properties = new[]
                {
                new PropertyByName<Donation>("Id", p => p.Id),
                new PropertyByName<Donation>("PurchasedWithOrderItemId", p => p.PurchasedWithOrderItem.Product.Name),
                new PropertyByName<Donation>("Amount", p => p.Amount),
                new PropertyByName<Donation>("DonorFirstName", p => p.DonorFirstName),
                new PropertyByName<Donation>("DonorLastName", p => p.DonorLastName),
                new PropertyByName<Donation>("DonorCompany", p => p.DonorCompany),
                new PropertyByName<Donation>("DonorAddress", p => p.DonorAddress),
                new PropertyByName<Donation>("DonorAddress2", p => p.DonorAddress2),
                new PropertyByName<Donation>("DonorCity", p => p.DonorCity),
                new PropertyByName<Donation>("DonorState", p => p.DonorState),
                new PropertyByName<Donation>("DonorZip", p => p.DonorZip),
                new PropertyByName<Donation>("DonorPhone", p => p.DonorPhone),
                new PropertyByName<Donation>("DonorCountry", p => p.DonorCountry),
                new PropertyByName<Donation>("Comments", p => p.Comments),
                new PropertyByName<Donation>("NotificationEmail", p => p.NotificationEmail),
                new PropertyByName<Donation>("OnBehalfOfFullName", p => p.OnBehalfOfFullName),
                new PropertyByName<Donation>("IsProcessed", p => p.IsProcessed),
                new PropertyByName<Donation>("IncludeGiftAmount", p => p.IncludeGiftAmount),
                new PropertyByName<Donation>("CreatedOnUtc", p => p.CreatedOnUtc),
                 };

                return ExportToXlsx(properties, donations);
            }
        }
        #endregion

        #region NU-64
        public virtual string ExportStoreCommissionsToXml(IList<StoreCommissionReportLineModel> commissions)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("StoreCommissions");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var commission in commissions)
            {
                xmlWriter.WriteStartElement("StoreCommission");

                xmlWriter.WriteElementString("OrderId", null, commission.OrderId.ToString());
                xmlWriter.WriteElementString("Store", null, commission.StoreName);
                xmlWriter.WriteElementString("Product", null, commission.ProductName);
                xmlWriter.WriteElementString("Vendor", null, commission.VendorName);
                xmlWriter.WriteElementString("Quantity", null, commission.Quantity.ToString());
                xmlWriter.WriteElementString("Rate", null, commission.Rate.ToString() + "%");
                xmlWriter.WriteElementString("Commission", null, commission.Commission);
                xmlWriter.WriteElementString("Earned", null, commission.Earned);
                xmlWriter.WriteElementString("TotalNoTaxes", null, commission.Total);
                xmlWriter.WriteElementString("Date", null, commission.Date);

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        public virtual void ExportStoreCommissionsToXlsx(Stream stream, IList<StoreCommissionReportLineModel> commissions)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.Add("Store Commissions");

                var properties = new[]
                    {
                        "OrderId",
                        "Store",
                        "Unit Number",
                        "Product",
                        "Vendor",
                        "Quantity",
                        "Rate",
                        "Commission",
                        "Earned",
                        "TotalNoTaxes",
                        "Date"
                    };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                int row = 2;
                foreach (var commission in commissions)
                {
                    int col = 1;

                    worksheet.Cells[row, col].Value = commission.OrderId; col++;
                    worksheet.Cells[row, col].Value = commission.StoreName; col++;
                    worksheet.Cells[row, col].Value = commission.UnitNumber; col++;
                    worksheet.Cells[row, col].Value = commission.ProductName; col++;
                    worksheet.Cells[row, col].Value = commission.VendorName; col++;
                    worksheet.Cells[row, col].Value = commission.Quantity; col++;
                    worksheet.Cells[row, col].Value = commission.Rate; col++;
                    worksheet.Cells[row, col].Value = commission.Commission; col++;
                    worksheet.Cells[row, col].Value = commission.Earned; col++;
                    worksheet.Cells[row, col].Value = commission.Total; col++;
                    worksheet.Cells[row, col].Value = commission.Date; col++;

                    row++;
                }

                xlPackage.Save();
            }
        }

        #endregion

        #region SODMYWAY-2948
        public virtual byte[] ExportOrdersToXlsx(IList<OrderByDeliveryReportLine> orders)
        {
            //property array
            var properties = new[]
                    {
                    new PropertyByName<OrderByDeliveryReportLine>("Unit", p => p.Unit),
                    new PropertyByName<OrderByDeliveryReportLine>("Category", p => p.Category),
                    new PropertyByName<OrderByDeliveryReportLine>("GL Code", p => p.GLCode),
                    new PropertyByName<OrderByDeliveryReportLine>("OrderId", p => p.OrderId),
                    new PropertyByName<OrderByDeliveryReportLine>("Delivery Date", p => p.OrdItmDeliveryDate),
                    new PropertyByName<OrderByDeliveryReportLine>("OrderSubTotalExclTax", p => p.OrderSubTotalExclTax),
                    new PropertyByName<OrderByDeliveryReportLine>("OrderTax", p => p.OrderTax),
                    new PropertyByName<OrderByDeliveryReportLine>("OrderSubtotalInclTax", p => p.OrderSubtotalInclTax)

            };

            return ExportToXlsx(properties, orders);
        }

        #endregion


        #region New Vertex

        //NEW TAX/SHIPPING
        /// <summary>
        /// Export orders for payment to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        public virtual void ExportPaymentReportToXlsxVERTEX(string filePath, IList<Order> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, bool isDelivery, bool isPerstore = false, DateTime? SDate = null, DateTime? EDate = null)
        {


            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);
                //Create Headers and format them
                string[] properties;
                if (isFuture)
                {
                    properties = new[]
                        {
                            "Store Name",
                            "Unit Number",
                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "GL1",
                            "Amount",
                            "Specialty GL",
                            "Amount",
                            "Delivery GL",
                            "Delivery Fee",
                            "Shipping GL",
                            "Shipping Fee",
                            "Totals",

                        };
                }
                else
                {
                    properties = new[]
                        {
                            "Store Name",
                            "Unit Number",
                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "GL1",
                            "Amount",
                            "GL2",
                            "Amount",
                            "GL3",
                            "Amount",
                            "Specialty GL",
                            "Amount",
                            "Delivery GL",
                            "Delivery Fee",
                            "Shipping GL",
                            "Shipping Fee",
                            "Tax GL1",
                            "Tax",
                            "Tax GL2",
                            "Tax",
                            "Tax GL3",
                            "Tax",
                            "Totals",

                        };
                }
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = reportName;
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;

                string glCode = string.Empty;
                int count = 1;
                int col = 1;

                string categoryName = string.Empty;
                IList<string> glAccountDescription = new List<string>();
                IList<string> glAccountCode = new List<string>();
                IList<string> glAccountTotal = new List<string>();



                var ccTotals = new Dictionary<string, decimal>();
                var glTotals = new Dictionary<string, decimal>();

                foreach (var order in orders)
                {

                    var cardType = "";
                    var cardTypeFromFirstData = order.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType") == null ? string.Empty : order.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType").ToLower(); //grab cardType for each order

                    if (cardTypeFromFirstData.ToLower() == "american express")
                    {
                        cardTypeFromFirstData = "amex";
                    }

                    if (cardTypeFromFirstData != null)
                    {
                        cardType = cardTypeFromFirstData.ToLower();
                    }

                    bool IsProcessed = false;
                    int OrderItemsCount = 0;
                    if (!isFuture)
                    {
                        if (order.StoreId == 12 || order.StoreId == 404)
                        {
                            return;
                        }
                        var orderItemdetails = order.OrderItems.Where(x => !x.Product.IsMealPlan && x.FulfillmentDateTime != null).ToList();
                        OrderItemsCount = orderItemdetails.Where(x => x.Product.IsLocalDelivery || x.Product.IsShipEnabled || x.Product.IsPickupEnabled).Count();
                    }
                    else
                    {
                        //var orderItemdetails = order.OrderItems.Where(x => !x.Product.IsMealPlan && x.FulfillmentDateTime == null).ToList();
                        var orderItemdetails = order.OrderItems.Where(x => !x.Product.IsMealPlan).ToList();
                        OrderItemsCount = orderItemdetails.Where(x => x.Product.IsLocalDelivery || x.Product.IsShipEnabled || x.Product.IsPickupEnabled).Count();
                    }
                    foreach (var orderItem in order.OrderItems) //loop through each order's items and build a row
                    {

                        int orderItemCount = order.OrderItems.Count();
                        int FulFilledOrder = order.OrderItems.Where(x => x.FulfillmentDateTime != null).Count();
                        var FullFilledItems = order.OrderItems.Where(x => x.FulfillmentDateTime != null).ToList();
                        var FullrefundedItemViaPartialRefund = order.OrderItems.Where(x => x.IsFullRefund).ToList();
                        decimal? nonFulfilledItemsDeliveryShippingCost = 0;
                        decimal? amountByFullRefund = 0;
                        foreach (var itemsummation in FullFilledItems)
                        {


                            if (itemsummation.IsDeliveryPickUp)
                                nonFulfilledItemsDeliveryShippingCost = nonFulfilledItemsDeliveryShippingCost + itemsummation.DeliveryPickupFee;
                            if (itemsummation.IsShipping)
                                nonFulfilledItemsDeliveryShippingCost = nonFulfilledItemsDeliveryShippingCost + itemsummation.ShippingFee;
                        }
                        foreach (var amountsForFullRefundToSubtract in FullrefundedItemViaPartialRefund)
                        {
                            if (amountsForFullRefundToSubtract.IsDeliveryPickUp)
                                amountByFullRefund = amountByFullRefund + amountsForFullRefundToSubtract.DeliveryPickupFee;
                            if (amountsForFullRefundToSubtract.IsShipping)
                                amountByFullRefund = amountByFullRefund + amountsForFullRefundToSubtract.ShippingFee;
                        }
                        var vertexOrderGls = _glCodeService.GetVertexGlBreakdown(order.Id, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax); //gets the order gls from vertex response

                        if (!isPerstore)
                        {
                            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"]))
                            {
                                var storesToExclude = System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"].Split(',');
                                if (storesToExclude.Contains(order.StoreId.ToString()))
                                    continue;
                            }
                        }

                        AddPaymentRow(false, isFuture, worksheet, ref row, ref col, ccTotals, glTotals, orderItem, cardType, vertexOrderGls, isDelivery, SDate, EDate, IsProcessed, nonFulfilledItemsDeliveryShippingCost, amountByFullRefund);
                        if (order.OrderStatus == OrderStatus.Cancelled)
                        {
                            var vertexCancelledOrderGls = _glCodeService.GetVertexGlBreakdown(order.Id, orderItem.ProductId, TaxRequestType.ResponseDistributiveTaxRefund); //gets if cancelled order GLS from vertex response                      
                            AddPaymentRow(true, isFuture, worksheet, ref row, ref col, ccTotals, glTotals, orderItem, cardType, vertexCancelledOrderGls, isDelivery, SDate, EDate, IsProcessed, nonFulfilledItemsDeliveryShippingCost, amountByFullRefund);
                        }
                        if (OrderItemsCount > 1 /*&&*/ /*orderItem.FulfillmentDateTime == null*/ && !orderItem.IsFullRefund)
                            IsProcessed = true;
                    }


                    count++;
                }

                row++;

                col = 13;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By Card Type";


                col = 16;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By GL";
                row++;

                var summaryRowStart = row;

                //CreditCardTotals
                foreach (var cardType in ccTotals)
                {
                    col = 13;
                    worksheet.Cells[row, col].Value = cardType.Key;
                    col++;

                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = cardType.Value;
                    row++;
                }


                //Gl Code Totals
                foreach (var glcodeTotal in glTotals)
                {
                    col = 16;
                    worksheet.Cells[summaryRowStart, col].Value = glcodeTotal.Key;
                    col++;

                    worksheet.Cells[summaryRowStart, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[summaryRowStart, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[summaryRowStart, col].Value = glcodeTotal.Value;
                    summaryRowStart++;
                }

                xlPackage.Save();
            }
        }

        private void AddPaymentRow(bool isCancelled, bool isFuture, ExcelWorksheet worksheet, ref int row, ref int col, Dictionary<string, decimal> ccTotals, Dictionary<string, decimal> glTotals, OrderItem orderItem, string cardType, List<DynamicVertexGLCode> vertexOrderGls, bool isDelivery, DateTime? startDate, DateTime? endDate, bool processed = false, decimal? nonFulfilledItemsDeliveryShippingCost = 0, decimal? amountByFullRefund = 0)
        {

            var order = orderItem.Order;  //
            int orderItemCount = order.OrderItems.Count();
            int nonFulFilledOrder = order.OrderItems.Where(x => x.FulfillmentDateTime == null).Count();

            if (order.Deleted || orderItem.Product.Deleted) // checking if the product or the order is deleted or not
            {
                return;
            }

            if (isDelivery)
            {
                if (orderItem.Product.IsMealPlan || orderItem.FulfillmentDateTime == null)
                {
                    return;
                }
            }

            if (isFuture)  // Checking for Future Delivery or Not
            {
                if (orderItem.Product.IsMealPlan)
                {
                    return;
                }
                if (order.OrderStatus == OrderStatus.Cancelled || orderItem.IsPartialRefund)
                {
                    return;
                }
                if (orderItem.FulfillmentDateTime != null)
                {
                    if (orderItem.FulfillmentDateTime >= startDate && orderItem.FulfillmentDateTime <= endDate)
                        return;

                }
            }

            col = 1;

            List<ProductGlCode> glCodes = new List<ProductGlCode>();
            List<ProductGlCode> splitGls = new List<ProductGlCode>();
            string deliveryGL = String.Empty;
            string shippingGL = String.Empty;

            string gl1 = "";
            decimal gl1Amount = 0;

            string gl2 = "";
            decimal gl2Amount = 0;

            string gl3 = "";
            decimal gl3Amount = 0;


            decimal DeliveryShippingAmount = 0;

            bool SetDeliveryAndShippingValue = false;
            if ((orderItem.FulfillmentDateTime != null || orderItem.Product.IsMealPlan))
            {
                List<ProductGlCode> nondistinct = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Deferred); //main GLS
                glCodes = nondistinct.GroupBy(x => x.GlCodeId, (key, group) => group.First()).ToList();
                splitGls = _glCodeService.GetProductsGlByType(1, glCodes, false, true);
            }
            else
            {
                List<ProductGlCode> nondistinct = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Deferred); //main GLS
                glCodes = nondistinct.GroupBy(x => x.GlCodeId, (key, group) => group.First()).ToList();
                splitGls = _glCodeService.GetProductsGlByType(1, glCodes, false, true);
            }

            if (glCodes.Count > 0)
            {
                if ((orderItem.FulfillmentDateTime != null || orderItem.Product.IsMealPlan))
                {
                    if (_glCodeService.GetProductGlCodeByName("Delivery Fee Onsite", glCodes) != null)
                    {
                        deliveryGL = _glCodeService.GetProductGlCodeByName("Delivery Fee Onsite", glCodes).GlCode.GlCodeName;
                    }
                    if (_glCodeService.GetProductGlCodeByName("Shipping Fee", glCodes) != null)
                    {
                        shippingGL = _glCodeService.GetProductGlCodeByName("Shipping Fee", glCodes).GlCode.GlCodeName;
                    }
                }

                //Split GLS
                if (splitGls.Count > 0)
                {
                    if (splitGls.Count == 1)
                    {
                        gl1 = splitGls[0].GlCode.GlCodeName;
                        gl1Amount = Math.Round(Convert.ToDecimal(splitGls[0].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1)), 2);


                    }
                    if (splitGls.Count == 2)
                    {
                        gl1 = splitGls[0].GlCode.GlCodeName;
                        gl1Amount = splitGls[0].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1);

                        //if (splitGls[1].GlCode.GlCodeName != deliveryGL && splitGls[1].GlCode.GlCodeName != shippingGL)
                        //{
                        //    if (orderItem.Product.IsShipEnabled && !isFuture)
                        //    {
                        //        gl2 = splitGls[1].GlCode.GlCodeName;
                        //        gl2Amount = order.OrderShippingInclTax * (isCancelled ? -1 : 1);
                        //    }
                        //    else if (!isFuture)
                        //    {
                        //        gl2 = splitGls[1].GlCode.GlCodeName;
                        //        gl2Amount = splitGls[1].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1);
                        //    }
                        //}
                        if (splitGls[1].GlCode.GlCodeName != deliveryGL && splitGls[1].GlCode.GlCodeName != shippingGL && !isFuture)
                        {
                            gl2 = splitGls[1].GlCode.GlCodeName;
                            gl2Amount = Math.Round(Convert.ToDecimal(splitGls[1].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1)), 2);
                        }
                    }
                    if (splitGls.Count == 3)
                    {
                        gl1 = splitGls[0].GlCode.GlCodeName;
                        gl1Amount = Math.Round(Convert.ToDecimal(splitGls[0].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1)), 2);

                        if (splitGls[1].GlCode.GlCodeName != deliveryGL && splitGls[1].GlCode.GlCodeName != shippingGL && !isFuture)
                        {
                            gl2 = splitGls[1].GlCode.GlCodeName;
                            gl2Amount = Math.Round(Convert.ToDecimal(splitGls[1].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1)), 2);
                        }
                        if (splitGls[2].GlCode.GlCodeName != deliveryGL && splitGls[2].GlCode.GlCodeName != shippingGL && !isFuture)
                        {
                            gl3 = splitGls[2].GlCode.GlCodeName;
                            gl3Amount = Math.Round(Convert.ToDecimal(splitGls[2].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1)), 2);
                        }


                    }
                }

            }


            if (gl1Amount != 0)
            {
                //store name

                worksheet.Cells[row, col].Value = _storeService.GetStoreById(orderItem.Order.StoreId).Name;
                col++;

                //unit id
                worksheet.Cells[row, col].Value = _storeService.GetStoreById(orderItem.Order.StoreId).ExtKey;
                col++;

                //Product Name
                worksheet.Cells[row, col].Value = orderItem.Product.Name;
                col++;

                //Order Id
                worksheet.Cells[row, col].Value = order.Id;
                col++;

                //Order Status for debugging
                //worksheet.Cells[row, col].Value = Enum.GetName(typeof(OrderStatus), order.OrderStatus);
                //col++;

                //Created on date
                worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToShortDateString();
                col++;
                worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";


                //Requested FulfillmentDate
                String values = "";
                if (!orderItem.RequestedFulfillmentDateTime.IsNullOrDefault())
                {
                    values = orderItem.RequestedFulfillmentDateTime.GetValueOrDefault().ToShortDateString();
                }
                worksheet.Cells[row, col].Value = values;
                col++;


                //Don't display card type now until we determine how to break out payments per order (VGC, MC)
                //Card Type
                worksheet.Cells[row, col].Value = cardType;
                col++;

                //Cost
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = orderItem.PriceExclTax * (isCancelled ? -1 : 1);
                col++;
                // orderSiteProductPriceExclTax += orderItem.PriceExclTax * (isCancelled ? -1 : 1); ; //report total

                //GL1
                worksheet.Cells[row, col].Value = gl1;
                col++;

                //GL1 Amount
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = Convert.ToString(gl1Amount) == "0" ? string.Empty : Convert.ToString(gl1Amount);
                col++;

                if (!isFuture)
                {
                    //GL2   
                    worksheet.Cells[row, col].Value = gl2;
                    col++;

                    //GL2 Amount
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = Convert.ToString(gl2Amount) == "0" ? string.Empty : Convert.ToString(gl2Amount);
                    col++;

                    //GL3   
                    worksheet.Cells[row, col].Value = gl3;
                    col++;

                    //GL3 Amount
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = Convert.ToString(gl3Amount) == "0" ? string.Empty : Convert.ToString(gl3Amount); ;
                    col++;

                }

                //Specialty GL
                worksheet.Cells[row, col].Value = string.Empty;
                col++;

                //Specialty GL Amount
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = string.Empty;
                col++;

                if (order.OrderShippingInclTax != 0)
                {

                    if ((orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) && isFuture && !SetDeliveryAndShippingValue)
                    {

                        //Delivery GL
                        if (!processed)
                            worksheet.Cells[row, col].Value = "48702040";
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        SetDeliveryAndShippingValue = true;
                        col++;

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {
                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                worksheet.Cells[row, col - 1].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : "48702040";
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;
                    }
                    if ((orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) && string.IsNullOrEmpty(deliveryGL) && !isFuture && !SetDeliveryAndShippingValue)
                    {
                        //Delivery GL
                        if (!processed)
                            worksheet.Cells[row, col].Value = "70161010";
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        SetDeliveryAndShippingValue = true;
                        col++;

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {
                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                    }
                    else if ((orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) && !string.IsNullOrEmpty(deliveryGL) && !isFuture && !SetDeliveryAndShippingValue)
                    {
                        if (!processed)
                            worksheet.Cells[row, col].Value = deliveryGL;
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        SetDeliveryAndShippingValue = true;
                        col++;

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {
                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                    }
                    else if (SetDeliveryAndShippingValue == false)
                    {
                        col++;

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {
                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                    }

                    //Don't display delivery per item since it is assigned per order
                    //Delivery Fee Amount

                    //SetDeliveryAndShippingValue = false;

                    if (orderItem.Product.IsShipEnabled && isFuture && !SetDeliveryAndShippingValue)
                    {
                        if (!processed)
                            worksheet.Cells[row, col].Value = "48702040";
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        SetDeliveryAndShippingValue = true;
                        col++;

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {

                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                worksheet.Cells[row, col - 1].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : "48702040";
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                    }
                    if (orderItem.Product.IsShipEnabled && string.IsNullOrEmpty(shippingGL) && !isFuture && !SetDeliveryAndShippingValue)
                    {
                        //Shipping GL
                        if (!processed)
                            worksheet.Cells[row, col].Value = "62610001";
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        SetDeliveryAndShippingValue = true;
                        col++;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {

                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                    }
                    else if (orderItem.Product.IsShipEnabled && !string.IsNullOrEmpty(shippingGL) && !isFuture && !SetDeliveryAndShippingValue)
                    {
                        if (!processed)
                            worksheet.Cells[row, col].Value = shippingGL;
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        SetDeliveryAndShippingValue = true;
                        col++;

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {

                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;


                    }
                    else if (SetDeliveryAndShippingValue == false)
                    {
                        col++;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        if (!processed)
                        {

                            var val = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? order.OrderShippingInclTax * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1)), 2));
                            worksheet.Cells[row, col].Value = (val == "0" || val == "0.00") ? string.Empty : val;
                            DeliveryShippingAmount = Convert.ToDecimal(val);
                            if ((orderItemCount > nonFulFilledOrder) && amountByFullRefund == decimal.Zero)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - nonFulfilledItemsDeliveryShippingCost);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                            else if (amountByFullRefund != 0)
                            {
                                var subtractedAmount = Convert.ToDecimal(order.OrderShippingInclTax - amountByFullRefund);
                                var val1 = Convert.ToString(Math.Round(SetDeliveryAndShippingValue ? subtractedAmount * (isCancelled ? -1 : 1) : (string.IsNullOrEmpty(shippingGL) ? 0 : subtractedAmount * (isCancelled ? -1 : 1)), 2));
                                worksheet.Cells[row, col].Value = (val1 == "0" || val1 == "0.00") ? string.Empty : val1;
                                DeliveryShippingAmount = Convert.ToDecimal(val1);
                            }
                        }
                        else
                            worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                    }
                }// section to omit records if the amount is a zero dollar entry
                //Don't display delivery per item since it is assigned per order
                //Shipping Fee Amount

                SetDeliveryAndShippingValue = false;

                var productVertexGls = _glCodeService.GetVertexProductGlBreakdown(vertexOrderGls, orderItem.ProductId.ToString());

                if (productVertexGls.Count > 0) //do we have any TAX GLS Coming from vertex?
                {
                    foreach (var taxGl in productVertexGls)
                    {

                        ////add to grand totals
                        //if (taxGl.Total != 0)
                        //{
                        //    if (!glTotals.ContainsKey(taxGl.GlCode))
                        //    {
                        //        glTotals.Add(taxGl.GlCode, taxGl.Total * (isCancelled ? -1 : 1));
                        //    }
                        //    else
                        //    {
                        //        glTotals[taxGl.GlCode] += taxGl.Total * (isCancelled ? -1 : 1);
                        //    }
                        //}


                        //taxGL
                        worksheet.Cells[row, col].Value = string.Empty;
                        col++;

                        //Product Tax

                        var tax = string.Empty;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = tax; //* (isCancelled ? -1 : 1);
                        col++;
                        //orderSiteProductTax += taxGl.Total;
                    }
                }
                else //we didn't use vertex to calculate taxes, there are no vertex tax GLS 
                {

                    //string taxGL = "44571098";
                    string taxGL = string.Empty;
                    //taxGL
                    //worksheet.Cells[row, col].Value = taxGL;
                    worksheet.Cells[row, col].Value = string.Empty;
                    col++;

                    //Product Tax

                    var tax = orderItem.PriceInclTax - orderItem.PriceExclTax;
                    //worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    // worksheet.Cells[row, col].Value = tax * (isCancelled ? -1 : 1);
                    worksheet.Cells[row, col].Value = string.Empty;
                    col++;

                    //add to grand totals
                    //if (tax != 0)
                    //{
                    //    if (!glTotals.ContainsKey(taxGL))
                    //    {
                    //        glTotals.Add(taxGL, tax * (isCancelled ? -1 : 1));
                    //    }
                    //    else
                    //    {
                    //        glTotals[taxGL] += tax * (isCancelled ? -1 : 1);
                    //    }
                    //}


                }

                decimal priceIncltax = 0;
                if (isFuture)
                {
                    if (orderItemCount > nonFulFilledOrder && !processed)
                    {
                        priceIncltax = ((orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery || orderItem.Product.IsPickupEnabled) && !processed) ? ((orderItem.PriceExclTax) + (DeliveryShippingAmount)) * (isCancelled ? -1 : 1) : ((orderItem.PriceExclTax)) * (isCancelled ? -1 : 1);
                        var valuedata = Convert.ToDecimal(priceIncltax - nonFulfilledItemsDeliveryShippingCost);
                        if (valuedata < 0)
                            priceIncltax = Convert.ToDecimal(priceIncltax - nonFulfilledItemsDeliveryShippingCost);

                    }
                    else if (amountByFullRefund != 0)
                    {
                        //(order.OrderShippingInclTax)
                        priceIncltax = ((orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery || orderItem.Product.IsPickupEnabled) && !processed) ? ((orderItem.PriceExclTax) + (DeliveryShippingAmount)) * (isCancelled ? -1 : 1) : ((orderItem.PriceExclTax)) * (isCancelled ? -1 : 1);
                        //var valuedata = Convert.ToDecimal(priceIncltax - amountByFullRefund);
                        //if (valuedata > decimal.Zero)
                        //    priceIncltax = valuedata;
                    }
                    else
                    {
                        priceIncltax = ((orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery || orderItem.Product.IsPickupEnabled) && !processed) ? ((orderItem.PriceExclTax) + (order.OrderShippingInclTax)) * (isCancelled ? -1 : 1) : ((orderItem.PriceExclTax)) * (isCancelled ? -1 : 1);

                    }

                }
                else
                    priceIncltax = ((orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery || orderItem.Product.IsPickupEnabled) && !processed) ? ((orderItem.PriceInclTax) + (order.OrderShippingInclTax)) * (isCancelled ? -1 : 1) : ((orderItem.PriceInclTax)) * (isCancelled ? -1 : 1);

                worksheet.Cells[row, 17].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, 17].Value = priceIncltax < decimal.Zero ? ((orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery || orderItem.Product.IsPickupEnabled) && !processed) ? ((orderItem.PriceExclTax) + (DeliveryShippingAmount)) * (isCancelled ? -1 : 1) : ((orderItem.PriceExclTax)) * (isCancelled ? -1 : 1) : priceIncltax;
                col++;

                //Delivery grand totals
                if (orderItem.DeliveryAmountExclTax > 0)
                {
                    if (!glTotals.ContainsKey(deliveryGL))
                    {
                        glTotals.Add(deliveryGL, orderItem.DeliveryAmountExclTax * (isCancelled ? -1 : 1));
                    }
                    else
                    {
                        glTotals[deliveryGL] += orderItem.DeliveryAmountExclTax * (isCancelled ? -1 : 1);
                    }
                }

                //Split Gl1 grand totals
                if (gl1Amount != 0)
                {
                    if (!glTotals.ContainsKey(gl1))
                    {
                        if (gl1 == "48702041")
                        {
                            if (!glTotals.ContainsKey("48702040"))
                            {
                                glTotals.Add("48702040", DeliveryShippingAmount);
                            }
                            else
                            {
                                glTotals["48702040"] += DeliveryShippingAmount;
                            }
                            glTotals.Add(gl1, gl1Amount);
                        }
                        else
                        {
                            glTotals.Add(gl1, gl1Amount + DeliveryShippingAmount);
                        }
                    }
                    else
                    {
                        if (gl1 == "48702041")
                        {
                            glTotals["48702040"] += DeliveryShippingAmount;
                            glTotals[gl1] += gl1Amount;
                        }
                        else
                        {
                            glTotals[gl1] += gl1Amount + DeliveryShippingAmount;
                        }
                    }
                }
                //Split Gl2 grand totals
                if (gl2Amount != 0)
                {
                    if (!glTotals.ContainsKey(gl2))
                    {
                        glTotals.Add(gl2, gl2Amount);
                    }
                    else
                    {
                        glTotals[gl2] += gl2Amount;
                    }
                }

                //split gl 3 grand totals
                if (gl3Amount != 0)
                {
                    if (!glTotals.ContainsKey(gl3))
                    {
                        glTotals.Add(gl3, gl3Amount);
                    }
                    else
                    {
                        glTotals[gl3] += gl3Amount;
                    }
                }




                //CC Totals
                if (!ccTotals.ContainsKey(cardType))
                {
                    ccTotals.Add(cardType, priceIncltax);
                }
                else
                {
                    ccTotals[cardType] += priceIncltax;
                }

                row++; //next Row of report

                //if (count < orders.Count)
            }
        }

        public virtual void ExportPaymentReport(string filePath, IList<Order> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, bool isDelivery)
        {


            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);
                //Create Headers and format them
                string[] properties;
                if (isFuture)
                {
                    properties = new[]
                        {
                            "Store Name",
                            "Unit Number",
                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "GL1",
                            "Amount",
                            "GL2",
                            "Amount",
                            "GL3",
                            "Amount",
                            "Specialty GL",
                            "Amount",
                            "Delivery GL",
                            "Delivery Fee",
                            "Shipping GL",
                            "Shipping Fee",
                            "Tax GL1",
                            "Tax",
                            "Tax GL2",
                            "Tax",
                            "Tax GL3",
                            "Tax",
                            "Totals",

                        };
                }
                else
                {
                    properties = new[]
                        {
                            "Store Name",
                            "Unit Number",
                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "GL1",
                            "Amount",
                            "GL2",
                            "Amount",
                            "GL3",
                            "Amount",
                            "Specialty GL",
                            "Amount",
                            "Delivery GL",
                            "Delivery Fee",
                            "Shipping GL",
                            "Shipping Fee",
                            "Tax GL1",
                            "Tax",
                            "Tax GL2",
                            "Tax",
                            "Tax GL3",
                            "Tax",
                            "Totals",

                        };
                }
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = reportName;
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;

                string glCode = string.Empty;
                int count = 1;
                int col = 1;

                string categoryName = string.Empty;
                IList<string> glAccountDescription = new List<string>();
                IList<string> glAccountCode = new List<string>();
                IList<string> glAccountTotal = new List<string>();



                var ccTotals = new Dictionary<string, decimal>();
                var glTotals = new Dictionary<string, decimal>();

                foreach (var order in orders)
                {
                    var cardType = "";
                    var cardTypeFromFirstData = order.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType") == null ? string.Empty : order.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType").ToLower(); //grab cardType for each order

                    if (cardTypeFromFirstData.ToLower() == "american express")
                    {
                        cardTypeFromFirstData = "amex";
                    }

                    if (cardTypeFromFirstData != null)
                    {
                        cardType = cardTypeFromFirstData.ToLower();
                    }
                    bool IsProcessed = false;
                    int OrderItemsCount = order.OrderItems.Where(x => x.Product.IsLocalDelivery || x.Product.IsShipEnabled || x.Product.IsPickupEnabled).Count();
                    foreach (var orderItem in order.OrderItems) //loop through each order's items and build a row
                    {
                        bool val = (orderItem.FulfillmentDateTime != null && !orderItem.Product.IsMealPlan);
                        var vertexOrderGls = _glCodeService.GetVertexGlBreakdown(order.Id, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax); //gets the order gls from vertex response


                        AddPaymentRow1(false, isFuture, worksheet, ref row, ref col, ccTotals, glTotals, orderItem, cardType, vertexOrderGls, isDelivery, val, IsProcessed);
                        if (order.OrderStatus == OrderStatus.Cancelled)
                        {
                            var vertexCancelledOrderGls = _glCodeService.GetVertexGlBreakdown(order.Id, orderItem.ProductId, TaxRequestType.ResponseDistributiveTaxRefund); //gets if cancelled order GLS from vertex response                      
                            AddPaymentRow1(true, isFuture, worksheet, ref row, ref col, ccTotals, glTotals, orderItem, cardType, vertexCancelledOrderGls, isDelivery, false, IsProcessed);
                        }
                        if (OrderItemsCount > 1)
                            IsProcessed = true;
                    }
                    count++;
                }

                row++;

                col = 13;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By Card Type";


                col = 16;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By GL";
                row++;

                var summaryRowStart = row;

                //CreditCardTotals
                foreach (var cardType in ccTotals)
                {
                    col = 13;
                    worksheet.Cells[row, col].Value = cardType.Key;
                    col++;

                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = cardType.Value;
                    row++;
                }


                //Gl Code Totals
                foreach (var glcodeTotal in glTotals)
                {
                    col = 16;
                    worksheet.Cells[summaryRowStart, col].Value = glcodeTotal.Key;
                    col++;

                    worksheet.Cells[summaryRowStart, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[summaryRowStart, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[summaryRowStart, col].Value = glcodeTotal.Value;
                    summaryRowStart++;
                }

                xlPackage.Save();
            }
        }

        private void AddPaymentRow1(bool isCancelled, bool isFuture, ExcelWorksheet worksheet, ref int row, ref int col, Dictionary<string, decimal> ccTotals, Dictionary<string, decimal> glTotals, OrderItem orderItem, string cardType, List<DynamicVertexGLCode> vertexOrderGls, bool isDelivery, bool refund = false, bool processed = false)
        {
            var order = orderItem.Order;  //
            if (order.Deleted || orderItem.Product.Deleted) // checking if the product or the order is deleted or not
            {
                return;
            }
            col = 1;
            List<ProductGlCode> glCodes = new List<ProductGlCode>();
            List<ProductGlCode> splitGls = new List<ProductGlCode>();
            List<ProductGlCode> nondistinct = new List<ProductGlCode>();
            string deliveryGL = String.Empty;
            string shippingGL = String.Empty;

            string gl1 = "";
            decimal gl1Amount = 0;

            string gl2 = "";
            decimal gl2Amount = 0;

            string gl3 = "";
            decimal gl3Amount = 0;

            if ((orderItem.FulfillmentDateTime != null && !orderItem.Product.IsMealPlan))
            {
                if (refund)
                {
                    nondistinct = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Deferred); //main GLS
                    glCodes = nondistinct.GroupBy(x => x.GlCodeId, (key, group) => group.First()).ToList();
                    splitGls = _glCodeService.GetProductsGlByType(1, glCodes, false, true);
                }
                else
                {
                    nondistinct = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid); //main GLS
                    glCodes = nondistinct.GroupBy(x => x.GlCodeId, (key, group) => group.First()).ToList();
                    splitGls = _glCodeService.GetProductsGlByType(1, glCodes, true, false);
                }
            }
            if ((orderItem.FulfillmentDateTime != null && orderItem.Product.IsMealPlan))
            {
                nondistinct = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid); //main GLS
                glCodes = nondistinct.GroupBy(x => x.GlCodeId, (key, group) => group.First()).ToList();
                splitGls = _glCodeService.GetProductsGlByType(1, glCodes, true, false);
            }
            else
            {
                nondistinct = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Deferred); //main GLS
                glCodes = nondistinct.GroupBy(x => x.GlCodeId, (key, group) => group.First()).ToList();
                splitGls = _glCodeService.GetProductsGlByType(1, glCodes, false, true);
            }

            if (glCodes.Count > 0)
            {
                if ((orderItem.FulfillmentDateTime != null && orderItem.Product.IsMealPlan))
                {
                    if (_glCodeService.GetProductGlCodeByName("Delivery Fee Onsite", glCodes) != null)
                    {
                        deliveryGL = _glCodeService.GetProductGlCodeByName("Delivery Fee Onsite", glCodes).GlCode.GlCodeName;
                    }
                    if (_glCodeService.GetProductGlCodeByName("Shipping Fee", glCodes) != null)
                    {
                        shippingGL = _glCodeService.GetProductGlCodeByName("Shipping Fee", glCodes).GlCode.GlCodeName;
                    }
                }

                //Split GLS
                if (splitGls.Count > 0)
                {
                    if (splitGls.Count == 1)
                    {
                        gl1 = splitGls[0].GlCode.GlCodeName;
                        gl1Amount = splitGls[0].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1);

                        if (!processed) // To remove duplicate records to be shown in Payment Report.
                        {
                            if (orderItem.Product.IsPickupEnabled || orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery)
                            {
                                gl2 = splitGls[0].GlCode.GlCodeName;
                                gl2Amount = order.OrderShippingInclTax * (isCancelled ? -1 : 1);
                            }

                            if ((orderItem.Product.IsPickupEnabled || orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery) && orderItem.Product.VendorId != 0)
                            {
                                gl2 = "48702040";
                                gl2Amount = order.OrderShippingInclTax * (isCancelled ? -1 : 1);
                            }
                        }
                    }
                    if (splitGls.Count == 2)
                    {
                        gl1 = splitGls[0].GlCode.GlCodeName;
                        gl1Amount = splitGls[0].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1);

                        if (!processed) // To remove duplicate records to be shown in Payment Report.
                        {
                            if (orderItem.Product.IsPickupEnabled || orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery)
                            {
                                gl2 = splitGls[1].GlCode.GlCodeName;
                                gl2Amount = order.OrderShippingInclTax * (isCancelled ? -1 : 1);
                            }
                        }
                        if (orderItem.Product.VendorId != 0)
                        {
                            gl2 = "48702040";
                            gl2Amount = order.OrderShippingInclTax * (isCancelled ? -1 : 1);
                        }

                        //if (splitGls[1].GlCode.GlCodeName != deliveryGL && splitGls[1].GlCode.GlCodeName != shippingGL)
                        //{
                        //    if (orderItem.Product.IsShipEnabled)
                        //    {
                        //        gl2 = splitGls[1].GlCode.GlCodeName;
                        //        gl2Amount = order.OrderShippingInclTax * (isCancelled ? -1 : 1);
                        //    }
                        //    else
                        //    {
                        //        gl2 = splitGls[1].GlCode.GlCodeName;
                        //        gl2Amount = splitGls[1].CalculateGLAmount(orderItem) * (isCancelled ? -1 : 1);
                        //    }
                        //}
                    }
                }

            }



            //store name

            worksheet.Cells[row, col].Value = _storeService.GetStoreById(orderItem.Order.StoreId).Name;
            col++;

            //unit id
            worksheet.Cells[row, col].Value = _storeService.GetStoreById(orderItem.Order.StoreId).ExtKey;
            col++;

            //Product Name
            worksheet.Cells[row, col].Value = orderItem.Product.Name;
            col++;

            //Order Id
            worksheet.Cells[row, col].Value = order.Id;
            col++;

            //Order Status for debugging
            //worksheet.Cells[row, col].Value = Enum.GetName(typeof(OrderStatus), order.OrderStatus);
            //col++;

            //Created on date
            worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToShortDateString();
            col++;
            worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";


            //Requested FulfillmentDate
            String values = "";
            if (!orderItem.RequestedFulfillmentDateTime.IsNullOrDefault())
            {
                values = orderItem.RequestedFulfillmentDateTime.GetValueOrDefault().ToShortDateString();
            }
            worksheet.Cells[row, col].Value = values;
            col++;


            //Don't display card type now until we determine how to break out payments per order (VGC, MC)
            //Card Type
            worksheet.Cells[row, col].Value = cardType;
            col++;

            //Cost
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = orderItem.PriceExclTax * (isCancelled ? -1 : 1);
            col++;
            // orderSiteProductPriceExclTax += orderItem.PriceExclTax * (isCancelled ? -1 : 1); ; //report total

            //GL1
            worksheet.Cells[row, col].Value = gl1;
            col++;

            //GL1 Amount
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = gl1Amount;
            col++;


            //GL2   
            worksheet.Cells[row, col].Value = gl2;
            col++;

            //GL2 Amount
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = gl2Amount;
            col++;

            //GL3   
            worksheet.Cells[row, col].Value = gl3;
            col++;

            //GL3 Amount
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = gl3Amount;
            col++;



            //Specialty GL
            worksheet.Cells[row, col].Value = "";
            col++;

            //Specialty GL Amount
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = 0;
            col++;


            if ((orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) && string.IsNullOrEmpty(deliveryGL) && orderItem.Product.IsMealPlan)
            {
                //Delivery GL
                worksheet.Cells[row, col].Value = "70161010";
                col++;
            }
            else
            {
                worksheet.Cells[row, col].Value = deliveryGL;
                col++;
            }



            //Don't display delivery per item since it is assigned per order
            //Delivery Fee Amount
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = string.IsNullOrEmpty(deliveryGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1);
            col++;

            if (orderItem.Product.IsShipEnabled && string.IsNullOrEmpty(deliveryGL) && orderItem.Product.IsMealPlan)
            {
                //Shipping GL
                worksheet.Cells[row, col].Value = "62610001";
                col++;

            }
            else
            {
                worksheet.Cells[row, col].Value = shippingGL;
                col++;
            }
            //Don't display delivery per item since it is assigned per order
            //Shipping Fee Amount
            worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, col].Value = string.IsNullOrEmpty(shippingGL) ? 0 : order.OrderShippingInclTax * (isCancelled ? -1 : 1);
            col++;

            var productVertexGls = _glCodeService.GetVertexProductGlBreakdown(vertexOrderGls, orderItem.ProductId.ToString());

            if (productVertexGls.Count > 0) //do we have any TAX GLS Coming from vertex?
            {
                foreach (var taxGl in productVertexGls)
                {

                    //add to grand totals
                    if (taxGl.Total != 0)
                    {
                        if (!glTotals.ContainsKey(taxGl.GlCode))
                        {
                            glTotals.Add(taxGl.GlCode, taxGl.Total * (isCancelled ? -1 : 1));
                        }
                        else
                        {
                            glTotals[taxGl.GlCode] += taxGl.Total;
                        }
                    }


                    //taxGL
                    worksheet.Cells[row, col].Value = taxGl.GlCode;
                    col++;

                    //Product Tax

                    var tax = taxGl.Total;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = tax; //* (isCancelled ? -1 : 1);
                    col++;
                    //orderSiteProductTax += taxGl.Total;
                }
            }
            else //we didn't use vertex to calculate taxes, there are no vertex tax GLS 
            {

                string taxGL = "44571098";
                //taxGL
                worksheet.Cells[row, col].Value = taxGL;
                col++;

                //Product Tax

                var tax = orderItem.PriceInclTax - orderItem.PriceExclTax;
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = tax * (isCancelled ? -1 : 1);
                col++;

                //add to grand totals
                if (tax != 0)
                {
                    if (!glTotals.ContainsKey(taxGL))
                    {
                        glTotals.Add(taxGL, tax * (isCancelled ? -1 : 1));
                    }
                    else
                    {
                        glTotals[taxGL] += tax * (isCancelled ? -1 : 1);
                    }
                }


            }

            decimal priceIncltax = ((orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery || orderItem.Product.IsShipEnabled) && processed) ? ((orderItem.PriceInclTax) + (order.OrderShippingInclTax)) * (isCancelled ? -1 : 1) : ((orderItem.PriceInclTax)) * (isCancelled ? -1 : 1);
            worksheet.Cells[row, 27].Style.Numberformat.Format = "$0.00";
            worksheet.Cells[row, 27].Value = priceIncltax;
            col++;

            //Delivery grand totals
            if (orderItem.DeliveryAmountExclTax > 0)
            {
                if (!glTotals.ContainsKey(deliveryGL))
                {
                    glTotals.Add(deliveryGL, orderItem.DeliveryAmountExclTax * (isCancelled ? -1 : 1));
                }
                else
                {
                    glTotals[deliveryGL] += orderItem.DeliveryAmountExclTax * (isCancelled ? -1 : 1);
                }
            }

            //Split Gl1 grand totals
            if (gl1Amount != 0)
            {
                if (!glTotals.ContainsKey(gl1))
                {
                    glTotals.Add(gl1, gl1Amount);
                }
                else
                {
                    glTotals[gl1] += gl1Amount;
                }
            }
            //Split Gl2 grand totals
            if (gl2Amount != 0)
            {
                if (!glTotals.ContainsKey(gl2))
                {
                    glTotals.Add(gl2, gl2Amount);
                }
                else
                {
                    glTotals[gl2] += gl2Amount;
                }
            }

            //split gl 3 grand totals
            if (gl3Amount != 0)
            {
                if (!glTotals.ContainsKey(gl3))
                {
                    glTotals.Add(gl3, gl3Amount);
                }
                else
                {
                    glTotals[gl3] += gl3Amount;
                }
            }




            //CC Totals
            if (!ccTotals.ContainsKey(cardType))
            {
                ccTotals.Add(cardType, priceIncltax);
            }
            else
            {
                ccTotals[cardType] += priceIncltax;
            }

            row++; //next Row of report

            //if (count < orders.Count)
        }


        //NEW TAX/SHIPPING
        /// <summary>
        /// Export orders for payment to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        public virtual void ExportDeliveryReportToXlsxVERTEX(string filePath, IList<Order> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {


            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);
                //Create Headers and format them
                string[] properties;
                if (isFuture)
                {
                    properties = new[]
                        {
                            "Store Name",
                            "Unit Number",
                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Fulfillment Date",
                            "Payment Type",
                            "Cost",
                            "GL1",
                            "Amount",
                            "GL2",
                            "Amount",
                            "Specialty GL",
                            "Amount",
                            "Delivery GL",
                            "Delivery Fee",
                            "Tax GL",
                            "Tax",
                            "Totals",

                        };
                }
                else
                {
                    properties = new[]
                        {
                            "Store Name",
                            "Unit Number",
                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Fulfillment Date",
                            "Payment Type",
                            "Cost",
                            "GL1",
                            "Amount",
                            "GL2",
                            "Amount",
                            "Specialty GL",
                            "Amount",
                            "Delivery GL",
                            "Delivery Fee",
                            "Tax GL",
                            "Tax",
                            "Totals",

                        };
                }
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "";
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;
                decimal paymentMethodAdditionalFeeInclTax = 0;

                string glCode = string.Empty;
                int count = 1;
                int col = 1;
                bool displaySectionTotal = false;
                string categoryName = string.Empty;
                decimal orderSiteProductTax = 0;
                decimal orderSiteProductPriceExclTax = 0;
                decimal orderSiteProductPriceInclTax = 0;
                int glAccountCount = 0;
                IList<string> glAccountDescription = new List<string>();
                IList<string> glAccountCode = new List<string>();
                IList<string> glAccountTotal = new List<string>();



                var ccTotals = new Dictionary<string, decimal>();
                var glTotals = new Dictionary<string, decimal>();

                foreach (var order in orders)
                {
                    var cardType = order.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType");

                    foreach (var orderItem in order.OrderItems.Where(orderItems => orderItems.Product.IsMealPlan == false))
                    {

                        col = 1;

                        List<ProductGlCode> glCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Deferred);

                        var splitGls = _glCodeService.GetProductsGlByType(1, glCodes, false, true);



                        string deliveryGL = "";
                        string taxGL = "";

                        string gl1 = "";
                        decimal gl1Amount = 0;
                        string gl2 = "";
                        decimal gl2Amount = 0;

                        if (glCodes.Count > 0)
                        {
                            if (_glCodeService.GetProductGlCodeByName("Delivery Fee", glCodes) != null)
                            {
                                deliveryGL = _glCodeService.GetProductGlCodeByName("Delivery Fee", glCodes).GlCode.GlCodeName;
                            }

                            if (_glCodeService.GetProductGlCodeByName("Tax", glCodes) != null)
                            {
                                taxGL = _glCodeService.GetProductGlCodeByName("Tax", glCodes).GlCode.GlCodeName;
                            }

                            if (_glCodeService.GetProductGlCodeByName("Tax", glCodes) != null)
                            {
                                taxGL = _glCodeService.GetProductGlCodeByName("Tax", glCodes).GlCode.GlCodeName;
                            }

                            //Split GLS
                            if (splitGls.Count > 0)
                            {
                                if (splitGls.Count == 1)
                                {
                                    gl1 = splitGls[0].GlCode.GlCodeName;
                                    gl1Amount = splitGls[0].CalculateGLAmount(orderItem);
                                }
                                if (splitGls.Count == 2)
                                {
                                    gl1 = splitGls[0].GlCode.GlCodeName;
                                    gl1Amount = splitGls[0].CalculateGLAmount(orderItem);

                                    gl2 = splitGls[1].GlCode.GlCodeName;
                                    gl2Amount = splitGls[1].CalculateGLAmount(orderItem);
                                }
                            }

                        }



                        worksheet.Cells[row, col].Value = _storeService.GetStoreById(orderItem.Order.StoreId).Name;
                        col++;

                        worksheet.Cells[row, col].Value = _storeService.GetStoreById(orderItem.Order.StoreId).ExtKey;
                        col++;

                        //Product Name
                        worksheet.Cells[row, col].Value = orderItem.Product.Name;
                        col++;

                        //Order Id
                        worksheet.Cells[row, col].Value = order.Id;
                        col++;

                        //Created on date
                        worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToShortDateString();
                        col++;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";


                        //FulfillmentDate
                        String values = "";
                        if (!orderItem.FulfillmentDateTime.IsNullOrDefault())
                        {
                            values = orderItem.FulfillmentDateTime.GetValueOrDefault().ToShortDateString();
                        }
                        worksheet.Cells[row, col].Value = values;
                        col++;


                        //Don't display card type now until we determine how to break out payments per order (VGC, MC)
                        //Card Type
                        worksheet.Cells[row, col].Value = cardType;
                        col++;

                        //Cost
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = orderItem.PriceExclTax;
                        col++;
                        orderSiteProductPriceExclTax = orderSiteProductPriceExclTax + orderItem.PriceExclTax; //report total

                        //GL1
                        worksheet.Cells[row, col].Value = gl1;
                        col++;

                        //GL1 Amount
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = gl1Amount;
                        col++;


                        //GL2   
                        worksheet.Cells[row, col].Value = gl2;
                        col++;

                        //GL2 Amount
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = gl2Amount;
                        col++;



                        //Specialty GL
                        worksheet.Cells[row, col].Value = "";
                        col++;

                        //Specialty GL Amount
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = 0;
                        col++;

                        //Delivery GL
                        worksheet.Cells[row, col].Value = deliveryGL;
                        col++;

                        //Don't display delivery per item since it is assigned per order
                        //Delivery Amount
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = orderItem.DeliveryAmountExclTax;
                        col++;



                        //taxGL
                        worksheet.Cells[row, col].Value = taxGL;
                        col++;

                        //Product Tax

                        var tax = decimal.Round(orderItem.PriceInclTax, 2) - decimal.Round(orderItem.PriceExclTax, 2); ;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = tax;
                        col++;
                        orderSiteProductTax = orderSiteProductTax + decimal.Round(orderItem.PriceInclTax, 2) - decimal.Round(orderItem.PriceExclTax, 2);

                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = orderItem.PriceInclTax;
                        col++;


                        //build Report Totals

                        //Tax GL Totals

                        if (tax > 0)
                        {
                            if (!glTotals.ContainsKey(taxGL))
                            {
                                glTotals.Add(taxGL, tax);
                            }
                            else
                            {
                                glTotals[taxGL] += tax;
                            }
                        }

                        //Delivery

                        if (orderItem.DeliveryAmountExclTax > 0)
                        {
                            if (!glTotals.ContainsKey(deliveryGL))
                            {
                                glTotals.Add(deliveryGL, orderItem.DeliveryAmountExclTax);
                            }
                            else
                            {
                                glTotals[deliveryGL] += orderItem.DeliveryAmountExclTax;
                            }
                        }

                        //Split Gl1
                        if (gl1Amount > 0)
                        {
                            if (!glTotals.ContainsKey(gl1))
                            {
                                glTotals.Add(gl1, gl1Amount);
                            }
                            else
                            {
                                glTotals[gl1] += gl1Amount;
                            }
                        }
                        //Split Gl2

                        if (gl2Amount > 0)
                        {
                            if (!glTotals.ContainsKey(gl2))
                            {
                                glTotals.Add(gl2, gl2Amount);
                            }
                            else
                            {
                                glTotals[gl2] += gl2Amount;
                            }
                        }

                        if (cardType != null)
                        {
                            //CC Totals
                            if (!ccTotals.ContainsKey(cardType))
                            {
                                ccTotals.Add(cardType, orderItem.PriceInclTax);
                            }
                            else
                            {
                                ccTotals[cardType] += orderItem.PriceInclTax;
                            }
                        }
                        orderSiteProductPriceInclTax = orderSiteProductPriceInclTax + orderItem.PriceInclTax;



                        row++; //next Row of report
                    }
                    //if (count < orders.Count)
                    //{
                    //    //if (glCode != orders[count].GLCode)
                    //    // {
                    //    displaySectionTotal = true;
                    //    //}
                    //}

                    //if (count > 0 && count == orders.Count)
                    //{
                    //    displaySectionTotal = true;
                    //}


                    if (displaySectionTotal)
                    {
                        glAccountCount++;

                        glAccountDescription.Add(categoryName);
                        glAccountCode.Add(glCode);
                        glAccountTotal.Add(String.Format("{0:C}", orderSiteProductPriceExclTax));

                        col = 1;


                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = "";
                        col++;


                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Value = "Total G/L " + glCode;
                        col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                        worksheet.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                        worksheet.Cells[row, col].Value = orderSiteProductPriceInclTax;

                        row++;

                        displaySectionTotal = false;
                        orderSiteProductPriceExclTax = 0;
                        orderSiteProductTax = 0;
                        orderSiteProductPriceInclTax = 0;


                    }
                    count++;
                }

                row++;
                col = 13;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By Card Type";


                col = 16;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By GL";

                row++;

                var summaryRowStart = row;

                //CreditCardTotals
                foreach (var cardType in ccTotals)
                {
                    col = 13;
                    worksheet.Cells[row, col].Value = cardType.Key;
                    col++;

                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = cardType.Value;
                    row++;
                }


                //Gl Code Totals
                foreach (var glcodeTotal in glTotals)
                {
                    col = 16;
                    worksheet.Cells[summaryRowStart, col].Value = glcodeTotal.Key;
                    col++;

                    worksheet.Cells[summaryRowStart, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[summaryRowStart, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[summaryRowStart, col].Value = glcodeTotal.Value;
                    summaryRowStart++;
                }



                //GL Totals
                //foreach (var cardType in cardTypes)
                //{
                //    col = 11;

                //    decimal orderDeliveryFee = orders.Where(c => c.CardType == cardType).Sum(export => export.OrderShippingExclTax);
                //    decimal ordersum = 0;//orders.Where(c => c.CardType == cardType).Sum(export => export.SiteProductTotal);

                //    worksheet.Cells[row, col].Value = cardType;
                //    col++;

                //    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                //    worksheet.Cells[row, col].Value = ordersum;
                //    row++;
                //}

                xlPackage.Save();
            }
        }

        #endregion


        #region Old Reporting
        /// <summary>
        /// Export orders for payment to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        public virtual void ExportPaymentReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);
                //Create Headers and format them
                string[] properties;
                if (isFuture)
                {
                    properties = new[]
                        {

                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "Delivery Fee",
                            "Tax",
                            "G/L Code",
                            "Totals",

                        };
                }
                else
                {
                    properties = new[]
                        {

                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Delivery Date",
                            "Payment Type",
                            "Cost",
                            "Delivery Fee",
                            "Tax",
                            "G/L Code",
                            "Totals",

                        };
                }
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "";
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;
                decimal paymentMethodAdditionalFeeInclTax = 0;

                string glCode = string.Empty;
                int count = 1;
                int col = 1;
                bool displaySectionTotal = false;
                string categoryName = string.Empty;
                IList<string> cardTypes = orders.Select(c => c.CardType).Distinct().ToList();
                decimal orderSiteProductTax = 0;
                decimal orderSiteProductPriceExclTax = 0;
                decimal orderSiteProductPriceInclTax = 0;
                int glAccountCount = 0;
                IList<string> glAccountDescription = new List<string>();
                IList<string> glAccountCode = new List<string>();
                IList<string> glAccountTotal = new List<string>();

                foreach (var order in orders)
                {

                    glCode = order.GLCode;
                    col = 1;

                    //worksheet.Cells[row, col].Value = "";// order.Id;
                    //col++;

                    categoryName = order.Category;

                    worksheet.Cells[row, col].Value = order.LocalProductName;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderId;
                    col++;

                    worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToShortDateString();
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";

                    String values = "";
                    if (!order.DeliveryDateUtc.IsNullOrDefault())
                    {
                        values = order.DeliveryDateUtc.GetValueOrDefault().ToShortDateString();
                    }

                    worksheet.Cells[row, col].Value = values;
                    col++;

                    //Don't display card type now until we determine how to break out payments per order (VGC, MC)
                    worksheet.Cells[row, col].Value = order.CardType;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductPriceExclTax;
                    col++;

                    orderSiteProductPriceExclTax = orderSiteProductPriceExclTax + order.SiteProductPriceExclTax;

                    //Don't display delivery per item since it is assigned per order
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.DeliveryAmountExclTax;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductTax;
                    col++;

                    orderSiteProductTax = orderSiteProductTax + order.SiteProductTax;


                    worksheet.Cells[row, col].Value = order.GLCode;
                    col++;



                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductTotal;
                    col++;

                    orderSiteProductPriceInclTax = orderSiteProductPriceInclTax + order.SiteProductTotal;




                    row++;

                    if (count < orders.Count)
                    {
                        //if (glCode != orders[count].GLCode)
                        //if (categoryName.ToLower().Trim() != orders[count].Category.ToLower().Trim())
                        if (glCode != orders[count].GLCode)
                        {
                            displaySectionTotal = true;
                        }
                    }

                    if (count > 0 && count == orders.Count)
                    {
                        displaySectionTotal = true;
                    }


                    if (displaySectionTotal)
                    {
                        glAccountCount++;

                        glAccountDescription.Add(categoryName);
                        glAccountCode.Add(glCode);
                        glAccountTotal.Add(String.Format("{0:C}", orderSiteProductPriceExclTax));

                        col = 1;


                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = ""; //orderSiteProductPriceExclTax;
                        col++;

                        //  worksheet.Cells[row, col].Style.Font.Bold = true;
                        // worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        // worksheet.Cells[row, col].Value = paymentMethodAdditionalFeeInclTax;
                        // col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        //worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = "Total G/L " + glCode;//orderSiteProductTax;
                        col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                        worksheet.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                        worksheet.Cells[row, col].Value = orderSiteProductPriceInclTax;

                        row++;

                        displaySectionTotal = false;
                        orderSiteProductPriceExclTax = 0;
                        orderSiteProductTax = 0;
                        orderSiteProductPriceInclTax = 0;


                    }
                    count++;
                }

                row++;
                col = 8;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Total By Card Type";

                row++;

                decimal glTotal;

                if (glAccountCount > 0)
                {
                    /*
                    for (int i = 0; i < glAccountCount; i++)
                    {
                        col = 1;
                        worksheet.Cells[row, col].Value = glAccountDescription[i];
                        col++;

                        worksheet.Cells[row, col].Value = glAccountCode[i];
                        col++;

                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Value = glAccountTotal[i];
                        row++;
                    }

                    string gl;
                    col = 1;
                    worksheet.Cells[row, col].Value = "Delivery Fee";
                    col++;

                    gl = glcodes.First(g => g.Description.ToLower() == "delivery fee").GlCode;

                    worksheet.Cells[row, col].Value = gl;
                    col++;

                    glTotal = orders.Sum(export => export.OrderShippingExclTax);
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = glTotal;
                    row++;


                    col = 1;
                    worksheet.Cells[row, col].Value = "Tax";
                    col++;

                    gl = glcodes.First(g => g.Description.ToLower() == "tax").GlCode;

                    worksheet.Cells[row, col].Value = gl;
                    col++;

                    glTotal = orders.Sum(export => export.SiteProductTax);
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = glTotal;
                    row++;

                    //List payment type totoals
                    row++;
                    col = 1;

                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Value = "Payment Type";
                    col++;

                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Value = "Totals";
                    col++;

                    row++;
                    */
                    foreach (var cardType in cardTypes)
                    {
                        col = 8;

                        decimal orderDeliveryFee = orders.Where(c => c.CardType == cardType).Sum(export => export.OrderShippingExclTax);
                        decimal ordersum = orders.Where(c => c.CardType == cardType).Sum(export => export.SiteProductTotal);

                        worksheet.Cells[row, col].Value = cardType;
                        col++;

                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = ordersum;// +orderDeliveryFee;
                        row++;
                    }
                }

                // we had better add some document properties to the spreadsheet 

                // set some core property values

                /*
                var sourceSiteSettings = _sourceSiteSettingsService.GetForSourceSiteId(_mySession.SourceSiteId);
                xlPackage.Workbook.Properties.Title = string.Format("{0} Payments", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Author = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} Payments", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} Payments", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Payments";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} Payments", sourceSiteSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(sourceSiteSettings.StoreUrl);
                */
                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        /// <summary>
        /// Export orders for delivery to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        public virtual void ExportDeliveryReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Daily Delivery Report");
                //Create Headers and format them
                var properties = new[]
                    {
                        "Order Id",
                        "Product Name",
                        "Payment Date",
                        "Delivery Date",
                        "Product Cost",
                        "Delivery Fee",
                        "Tax",
                        "Totals",
                    };

                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit: ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "Delivery Date Range: ";
                worksheet.Cells[3, 2].Value = reportRunDate;


                //beginning of main report

                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;

                decimal glOrderTotal = 0;
                decimal orderSiteProductTax = 0;
                decimal orderSiteProductTaxTotal = 0;
                decimal orderSiteProductDeliveryFee = 0;
                decimal orderSiteProductPriceExclTax = 0;
                decimal orderSiteProductPriceInclTax = 0;
                decimal paymentMethodAdditionalFeeInclTax = 0;
                string glCode = string.Empty;
                int count = 1;
                int index = 0;
                int records = 0;
                int col = 1;
                int startIndex = 0;
                bool displaySectionTotal = false;
                string categoryName = string.Empty;
                int glAccountCount = 0;



                List<ReportHelper> reportHelpers = new List<ReportHelper>();
                //   IList<string> glAccountDescription = new List<string>();
                //  IList<string> glAccountCode = new List<string>();
                //  IList<string> glAccountTotal = new List<string>();

                foreach (var exportRow in orders)
                {

                    //   if (String.IsNullOrEmpty(glCode))
                    glCode = exportRow.GLCode;

                    col = 1;

                    //Order Id
                    worksheet.Cells[row, col].Value = exportRow.OrderId;
                    col++;

                    worksheet.Cells[row, col].Value = exportRow.ProductName;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";
                    worksheet.Cells[row, col].Value = exportRow.PaidDateUtc.GetValueOrDefault().ToShortDateString();
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";
                    worksheet.Cells[row, col].Value = exportRow.DeliveryDateUtc.GetValueOrDefault().ToShortDateString();
                    col++;


                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = exportRow.SiteProductPriceExclTax;
                    col++;


                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = exportRow.DeliveryAmountExclTax;
                    orderSiteProductDeliveryFee += exportRow.DeliveryAmountExclTax;
                    col++;

                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = exportRow.SiteProductTax;
                    orderSiteProductTaxTotal += exportRow.SiteProductTax;
                    col++;

                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = exportRow.SiteProductTotal;
                    col++;

                    orderSiteProductPriceExclTax = orderSiteProductPriceExclTax + exportRow.SiteProductPriceExclTax;
                    orderSiteProductTax = orderSiteProductTax + exportRow.SiteProductTax;

                    orderSiteProductPriceInclTax = orderSiteProductPriceInclTax + exportRow.SiteProductTotal;

                    row++;

                    if (count < orders.Count)
                    {
                        //if (glCode != orders[count].GLCode)
                        if (categoryName.ToLower().Trim() != orders[count].Category.ToLower().Trim())
                        {
                            displaySectionTotal = true;
                            records = count;
                        }
                    }

                    if (count == orders.Count)
                    {
                        displaySectionTotal = true;
                    }

                    if (displaySectionTotal)
                    {
                        glAccountCount++;

                        bool glfound = false;

                        foreach (ReportHelper helper in reportHelpers)
                        {
                            if (helper.Name == glCode)
                            {
                                glfound = true;
                                helper.Total += orderSiteProductPriceExclTax;
                                break;
                            }
                        }

                        if (!glfound)
                        {
                            reportHelpers.Add(new ReportHelper(glCode, "", orderSiteProductPriceExclTax));
                        }



                        decimal grandTotal = 0;

                        for (int i = startIndex; i <= records - 1; i++)
                        {
                            grandTotal = grandTotal + orders[i].SiteProductPriceExclTax;
                        }
                        col = 1;
                        glCode = exportRow.GLCode;

                        orderSiteProductPriceExclTax = 0;
                        orderSiteProductTax = 0;
                        orderSiteProductPriceInclTax = 0;

                    }
                    count++;
                    index++;
                }

                row++;
                col = 1;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "G/L Account";

                worksheet.Cells[row + 1, col].Style.Font.Bold = true;
                worksheet.Cells[row + 1, col].Value = "Totals By G/L";

                foreach (GlCode code in glcodes)
                {
                    if (code.Description == "Tax" && code.IsDelivered)
                    {
                        reportHelpers.Add(new ReportHelper(code.GlCodeName, code.Description, orderSiteProductTaxTotal));
                    }
                    else if (code.Description == "Delivery Fee" && code.IsDelivered)
                    {
                        reportHelpers.Add(new ReportHelper(code.GlCodeName, code.Description, orderSiteProductDeliveryFee));
                    }
                }

                col++;

                decimal glTotal;

                if (glAccountCount > 0)
                {
                    col = 2;

                    for (int i = 0; i < reportHelpers.Count; i++)
                    {

                        if (reportHelpers[i].Description == "Delivery Fee")
                        {
                            col = 6;
                        }
                        else if (reportHelpers[i].Description == "Tax")
                        {
                            col = 7;
                        }

                        //worksheet.Cells[row, col].Value = glAccountDescription[i];
                        //col++;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.Font.UnderLine = true;
                        worksheet.Cells[row, col].Value = reportHelpers[i].Name;

                        //totals a row below the GL Account name
                        worksheet.Cells[row + 1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row + 1, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row + 1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row + 1, col].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        worksheet.Cells[row + 1, col].Value = String.Format("{0:C}", reportHelpers[i].Total);
                        col++;
                    }
                    row++;

                }

                row++;

                // we had better add some document properties to the spreadsheet 

                // set some core property values
                /*
                var sourceSiteSettings = _sourceSiteSettingsService.GetForSourceSiteId(_mySession.SourceSiteId);
                xlPackage.Workbook.Properties.Title = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Author = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Deliveries";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(sourceSiteSettings.StoreUrl);

                 */
                // save the new spreadsheet
                xlPackage.Save();
            }
        }


        /// <summary>
        /// Export orders for delivery to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        public virtual void ExportDeliveryByUnitReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                var worksheet = xlPackage.Workbook.Worksheets.Add("Deliveries");
                //Create Headers and format them
                var properties = new[]
                        {

                            "Unit",
                            "Category",
                            "GL Code",
                            "OrderId",
                            "Delivery Date",
                            "OrderSubTotalExclTax",
                            "OrderTax",
                            "OrderSubtotalInclTax",

                        };

                int orderCount = 0;

                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit: ";
                worksheet.Cells[1, 2].Value = "All";

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportStartDate = startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);
                string reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "Delivery Date Range: ";
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;
                decimal orderSiteProductTax = 0;
                decimal orderSiteProductPriceExclTax = 0;
                decimal orderSiteProductPriceInclTax = 0;

                string glCode = string.Empty;
                int count = 1;
                int index = 0;

                int col = 1;

                string categoryName = string.Empty;


                IList<OrderExcelExport> sortedOrders = orders.OrderBy(o => o.SiteExtKey).ToList();
                foreach (var order in sortedOrders)
                {
                    orderCount++;
                    if (String.IsNullOrEmpty(glCode))
                        glCode = order.GLCode;

                    col = 1;

                    worksheet.Cells[row, col].Value = order.SiteExtKey;
                    col++;

                    worksheet.Cells[row, col].Value = order.Category;
                    col++;

                    worksheet.Cells[row, col].Value = order.GLCode;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderId;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";
                    worksheet.Cells[row, col].Value = order.DeliveryDateUtc;
                    col++;

                    // worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductPriceExclTax;
                    col++;

                    orderSiteProductPriceExclTax = orderSiteProductPriceExclTax + order.SiteProductPriceExclTax;

                    //worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductTax;
                    col++;

                    orderSiteProductTax = orderSiteProductTax + order.SiteProductTax;

                    // worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductTotal;

                    orderSiteProductPriceInclTax = orderSiteProductPriceInclTax + order.SiteProductTotal;
                    row++;

                    count++;
                    index++;
                }

                row++;
                col = 1;

                col++;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Totals";
                col++;
                col++;
                col++;
                col++;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = sortedOrders.Sum(o => o.SiteProductPriceExclTax);
                col++;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = sortedOrders.Sum(o => o.SiteProductTax);
                col++;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Value = sortedOrders.Sum(o => o.SiteProductTotal);
                col++;

                row++;


                // set some core property values
                /*

                var sourceSiteSettings = _sourceSiteSettingsService.GetForSourceSiteId(_mySession.SourceSiteId);
                xlPackage.Workbook.Properties.Title = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Author = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Deliveries";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} Deliveries", sourceSiteSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(sourceSiteSettings.StoreUrl);
                */
                // save the new spreadsheet
                xlPackage.Save();

            }
        }

        /// <summary>
        /// Export orders for refund to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        public virtual void ExportRefundReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);
                //Create Headers and format them
                string[] properties;
                if (isFuture)
                {
                    properties = new[]
                        {

                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "Delivery Fee",
                            "Tax",
                            "Refunded",
                            "G/L Code",
                            "Totals",

                        };
                }
                else
                {
                    properties = new[]
                        {

                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Delivery Date",
                            "Payment Type",
                            "Cost",
                            "Delivery Fee",
                            "Tax",
                            "Refunded",
                            "G/L Code",
                            "Totals",

                        };
                }
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "Date Range: ";
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;

                string glCode = string.Empty;
                int count = 1;
                int col = 1;
                string categoryName = string.Empty;
                IList<string> cardTypes = orders.Select(c => c.CardType).Distinct().ToList();
                decimal orderSiteProductTax = 0;
                decimal orderSiteProductPriceExclTax = 0;
                decimal orderSiteProductPriceInclTax = 0;
                bool displayRefundWithGLCode = true;
                Dictionary<string, decimal> RefundAmountGlCodeDict = new Dictionary<string, decimal>();
                int glAccountCount = 0;
                IList<string> glAccountDescription = new List<string>();
                IList<string> glAccountCode = new List<string>();
                IList<string> glAccountTotal = new List<string>();
                decimal totalAmountRefunded = 0;

                foreach (var order in orders)
                {

                    glCode = order.GLCode;
                    col = 1;

                    //worksheet.Cells[row, col].Value = "";// order.Id;
                    //col++;

                    categoryName = order.Category;

                    worksheet.Cells[row, col].Value = order.LocalProductName;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderId;
                    col++;

                    worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToShortDateString();
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";

                    String values = "";
                    if (!order.DeliveryDateUtc.IsNullOrDefault())
                    {
                        values = order.DeliveryDateUtc.GetValueOrDefault().ToShortDateString();
                    }

                    worksheet.Cells[row, col].Value = values;
                    col++;

                    //Don't display card type now until we determine how to break out payments per order (VGC, MC)
                    worksheet.Cells[row, col].Value = order.CardType;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductPriceExclTax;
                    col++;

                    orderSiteProductPriceExclTax = orderSiteProductPriceExclTax + order.SiteProductPriceExclTax;

                    //Don't display delivery per item since it is assigned per order
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    //worksheet.Cells[row, col].Value = order.DeliveryAmountExclTax;
                    worksheet.Cells[row, col].Value = order.OrderShippingExclTax;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductTax;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax;
                    col++;

                    orderSiteProductTax = orderSiteProductTax + order.SiteProductTax;

                    worksheet.Cells[row, col].Value = order.GLCode;
                    col++;

                    if (RefundAmountGlCodeDict == null)
                    {
                        RefundAmountGlCodeDict.Add(order.GLCode, order.RefundedAmount);
                    }
                    else
                    {
                        if (RefundAmountGlCodeDict.ContainsKey(order.GLCode))
                        {
                            decimal oldRefund = RefundAmountGlCodeDict[order.GLCode];
                            RefundAmountGlCodeDict[order.GLCode] = oldRefund + (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax);
                            totalAmountRefunded = RefundAmountGlCodeDict[order.GLCode];
                        }
                        else
                        {
                            RefundAmountGlCodeDict.Add(order.GLCode, (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax));
                            totalAmountRefunded = RefundAmountGlCodeDict[order.GLCode];
                        }
                    }

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    //worksheet.Cells[row, col].Value = order.SiteProductTotal - order.RefundedAmount;
                    worksheet.Cells[row, col].Value = (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax) == 0 ? (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax) : -(order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax);
                    col++;

                    orderSiteProductPriceInclTax = orderSiteProductPriceInclTax + order.SiteProductTotal;

                    row++;

                    count++;
                }


                if (displayRefundWithGLCode)
                {

                    foreach (var glCodeItem in RefundAmountGlCodeDict.Keys)
                    {
                        glAccountCount++;
                        col = 1;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Value = "";
                        col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        //worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = "Refund Total G/L " + glCodeItem;//orderSiteProductTax;
                        col++;

                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Double;
                        worksheet.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                        worksheet.Cells[row, col].Value = RefundAmountGlCodeDict[glCodeItem];

                        row++;
                    }

                }

                row++;

                col = 8;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Refund By Card Type";

                row++;

                decimal glTotal;



                if (glAccountCount > 0)
                {
                    /*
                    for (int i = 0; i < glAccountCount; i++)
                    {
                        col = 1;
                        worksheet.Cells[row, col].Value = glAccountDescription[i];
                        col++;

                        worksheet.Cells[row, col].Value = glAccountCode[i];
                        col++;

                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Value = glAccountTotal[i];
                        row++;
                    }

                    string gl;
                    col = 1;
                    worksheet.Cells[row, col].Value = "Delivery Fee";
                    col++;

                    gl = glcodes.First(g => g.Description.ToLower() == "delivery fee").GlCode;

                    worksheet.Cells[row, col].Value = gl;
                    col++;

                    glTotal = orders.Sum(export => export.OrderShippingExclTax);
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = glTotal;
                    row++;


                    col = 1;
                    worksheet.Cells[row, col].Value = "Tax";
                    col++;

                    gl = glcodes.First(g => g.Description.ToLower() == "tax").GlCode;

                    worksheet.Cells[row, col].Value = gl;
                    col++;

                    glTotal = orders.Sum(export => export.SiteProductTax);
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = glTotal;
                    row++;

                    //List payment type totoals
                    row++;
                    col = 1;

                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Value = "Payment Type";
                    col++;

                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Value = "Totals";
                    col++;

                    row++;
                    */
                    foreach (var cardType in cardTypes)
                    {
                        col = 8;

                        decimal orderDeliveryFee = orders.Where(c => c.CardType == cardType).Sum(export => export.OrderShippingExclTax);
                        decimal orderRefundsum = orders.Where(c => c.CardType == cardType).Sum(export => export.RefundedAmount);

                        worksheet.Cells[row, col].Value = cardType;
                        col++;

                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        //worksheet.Cells[row, col].Value = orderRefundsum;// +orderDeliveryFee;
                        worksheet.Cells[row, col].Value = totalAmountRefunded;// +orderDeliveryFee;
                        row++;
                    }
                }

                // we had better add some document properties to the spreadsheet 

                // set some core property values

                /*
                var sourceSiteSettings = _sourceSiteSettingsService.GetForSourceSiteId(_mySession.SourceSiteId);
                xlPackage.Workbook.Properties.Title = string.Format("{0} Payments", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Author = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} Payments", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} Payments", sourceSiteSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Payments";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} Payments", sourceSiteSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = sourceSiteSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(sourceSiteSettings.StoreUrl);
                */
                // save the new spreadsheet
                xlPackage.Save();
            }
        }


        public virtual void ExportAllUnitsRefundReportToXlsx(string filePath, IList<OrderExcelExport> orders, List<CovidRefunds> covidOrders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);
                //Create Headers and format them
                string[] properties;
                if (isFuture)
                {
                    properties = new[]
                        {

                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Requested Delivery Date",
                            "Payment Type",
                            "Cost",
                            "Delivery Fee",
                            "Tax",
                            "Refunded",
                            "G/L Code",
                            "Totals",

                        };
                }
                else
                {
                    properties = new[]
                        {

                            "Product Name",
                            "OrderId",
                            "Payment Date",
                            "Delivery Date",
                            "Payment Type",
                            "Cost",
                            "Delivery Fee",
                            "Tax",
                            "Refunded",
                            "G/L Code",
                            "Totals",

                        };
                }
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = "All Units";

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "Date Range: ";
                worksheet.Cells[3, 2].Value = reportRunDate;


                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = properties[i];
                    worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                }

                int row = 6;

                string glCode = string.Empty;
                int count = 1;
                int col = 1;
                string categoryName = string.Empty;
                IList<string> cardTypes = orders.Select(c => c.CardType).Distinct().ToList();
                decimal orderSiteProductTax = 0;
                decimal orderSiteProductPriceExclTax = 0;
                decimal orderSiteProductPriceInclTax = 0;
                bool displayRefundWithGLCode = true;
                Dictionary<string, decimal> RefundAmountGlCodeDict = new Dictionary<string, decimal>();
                int glAccountCount = 0;
                IList<string> glAccountDescription = new List<string>();
                IList<string> glAccountCode = new List<string>();
                IList<string> glAccountTotal = new List<string>();
                decimal totalAmountRefunded = 0;

                Dictionary<string, decimal> amountPerCard = new Dictionary<string, decimal>();

                foreach (var order in orders)
                {
                    decimal covidRefundAmount=0;
                    decimal covidTax=0;

                    var covidRecord = covidOrders.Where(x => x.OrderId == order.OrderId && x.OrderItemId == order.OrderItemId).ToList();
                    if(covidRecord.Count > 0)
                    {
                        covidRefundAmount = covidRecord[0].GLAmount1 + covidRecord[0].GLAmount2 + covidRecord[0].GLAmount3;
                        covidTax = covidRecord[0].TaxAmount1 + covidRecord[0].TaxAmount2 + covidRecord[0].TaxAmount3;

                    }

                    glCode = order.GLCode;
                    col = 1;

                    //worksheet.Cells[row, col].Value = "";// order.Id;
                    //col++;

                    categoryName = order.Category;

                    worksheet.Cells[row, col].Value = order.LocalProductName;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderId;
                    col++;

                    worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToShortDateString();
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "MM/DD/YY";

                    String values = "";
                    if (!order.DeliveryDateUtc.IsNullOrDefault())
                    {
                        values = order.DeliveryDateUtc.GetValueOrDefault().ToShortDateString();
                    }

                    worksheet.Cells[row, col].Value = values;
                    col++;

                    //Don't display card type now until we determine how to break out payments per order (VGC, MC)
                    worksheet.Cells[row, col].Value = order.CardType;
                    col++;

                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    worksheet.Cells[row, col].Value = order.SiteProductPriceExclTax;
                    col++;

                    orderSiteProductPriceExclTax = orderSiteProductPriceExclTax + order.SiteProductPriceExclTax;

                    //Don't display delivery per item since it is assigned per order
                    worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                    //worksheet.Cells[row, col].Value = order.DeliveryAmountExclTax;
                    worksheet.Cells[row, col].Value = order.OrderShippingExclTax;  //Delivery fee
                    col++;


                    
                    if (covidRecord.Count>0)
                    {
                        //Tax
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = covidTax;
                        col++;

                        //Refund
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = covidRefundAmount;
                        col++;

                        
                    }
                    else
                    {
                        //Tax
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = order.SiteProductTax;
                        col++;

                        //Refund
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = order.RefundedAmount;

                        //worksheet.Cells[row, col].Value = order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax;
                        

                        col++;
                    }

                    orderSiteProductTax = orderSiteProductTax + order.SiteProductTax;

                    worksheet.Cells[row, col].Value = order.GLCode;
                    col++;

                    if(covidRecord.Count > 0)
                    {
                        totalAmountRefunded += covidRefundAmount + covidTax;
                    }
                    else
                    {
                        totalAmountRefunded += order.SiteProductTax + order.RefundedAmount;
                        //totalAmountRefunded += order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax;
                    }

                    //if (RefundAmountGlCodeDict == null)
                    //{
                    //    RefundAmountGlCodeDict.Add(order.GLCode, order.RefundedAmount);
                    //}
                    //else
                    //{
                    //    if (RefundAmountGlCodeDict.ContainsKey(order.GLCode))
                    //    {
                    //        decimal oldRefund = RefundAmountGlCodeDict[order.GLCode];
                    //        RefundAmountGlCodeDict[order.GLCode] = oldRefund + (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax);
                    //        totalAmountRefunded = RefundAmountGlCodeDict[order.GLCode];
                    //    }
                    //    else
                    //    {
                    //        RefundAmountGlCodeDict.Add(order.GLCode, (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax));
                    //        totalAmountRefunded = RefundAmountGlCodeDict[order.GLCode];
                    //    }
                    //}


                    //total refund
                    if (covidRecord.Count > 0)
                    {
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = -(covidRefundAmount + covidTax);
                        col++;

                        if (amountPerCard.ContainsKey(order.CardType))
                        {
                            amountPerCard[order.CardType] += covidRefundAmount + covidTax;
                        }
                        else
                        {
                            amountPerCard.Add(order.CardType, covidRefundAmount + covidTax);
                        }


                    }
                    else
                    {
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = -(order.SiteProductTax + order.RefundedAmount);

                        //worksheet.Cells[row, col].Value = (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax) == 0 ? (order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax) : -(order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax);
                        //worksheet.Cells[row, col].Value = order.SiteProductTotal - order.RefundedAmount;
                        col++;

                        if (amountPerCard.ContainsKey(order.CardType))
                        {
                            amountPerCard[order.CardType] += order.SiteProductTax + order.RefundedAmount; // order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax; 
                        }
                        else
                        {
                            amountPerCard.Add(order.CardType, order.SiteProductTax + order.RefundedAmount);                            
                            //amountPerCard.Add(order.CardType, order.SiteProductPriceExclTax + order.OrderShippingExclTax + order.SiteProductTax);
                        }

                    }

                    orderSiteProductPriceInclTax = orderSiteProductPriceInclTax + order.SiteProductTotal;

                    row++;

                    count++;
                }


                col = 8;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, col].Value = "Refund Total G/L ";
                col++;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Double;
                worksheet.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                worksheet.Cells[row, col].Value = totalAmountRefunded;
                row++;

                //if (displayRefundWithGLCode)
                //{

                //    foreach (var glCodeItem in RefundAmountGlCodeDict.Keys)
                //    {
                //        glAccountCount++;
                //        col = 1;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Value = "";
                //        col++;

                //        worksheet.Cells[row, col].Style.Font.Bold = true;
                //        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //        //worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                //        worksheet.Cells[row, col].Value = "Refund Total G/L " + glCodeItem;//orderSiteProductTax;
                //        col++;

                //        worksheet.Cells[row, col].Style.Font.Bold = true;
                //        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                //        worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Double;
                //        worksheet.Cells[row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                //        worksheet.Cells[row, col].Value = totalAmountRefunded; //RefundAmountGlCodeDict[glCodeItem];

                //        row++;
                //    }

                //}

                row++;

                col = 8;

                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Value = "Refund By Card Type";

                row++;

                decimal glTotal;

                if(amountPerCard.Count >0)
                {
                    foreach(var cardAmount in amountPerCard)
                    {
                        col = 8;
                        worksheet.Cells[row, col].Value = cardAmount.Key;
                        col++;

                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                        worksheet.Cells[row, col].Value = cardAmount.Value;
                        row++;

                    }
                }

                //if (glAccountCount > 0)
                //{
                //    foreach (var cardType in cardTypes)
                //    {
                //        col = 8;

                //        decimal orderDeliveryFee = orders.Where(c => c.CardType == cardType).Sum(export => export.OrderShippingExclTax);
                //        decimal orderRefundsum = orders.Where(c => c.CardType == cardType).Sum(export => export.RefundedAmount);

                //        worksheet.Cells[row, col].Value = cardType;
                //        col++;

                //        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //        worksheet.Cells[row, col].Style.Numberformat.Format = "$0.00";
                //        //worksheet.Cells[row, col].Value = orderRefundsum;// +orderDeliveryFee;
                //        worksheet.Cells[row, col].Value = totalAmountRefunded;// +orderDeliveryFee;
                //        row++;
                //    }
                //}
             xlPackage.Save();
            }
        }

        #endregion

        #region JE Reports
        public void ExportJournalReporttoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                int rowCount = 0;
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                var worksheet = xlPackage.Workbook.Worksheets.Add("JournalReports");

                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit Number";

                worksheet.Cells[1, 2].Style.Font.Bold = true;
                worksheet.Cells[1, 2].Value = "Unit Name";

                worksheet.Cells[1, 3].Style.Font.Bold = true;
                worksheet.Cells[1, 3].Value = "Unit Number State";


                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Value = "Unit Number Zipcode";

                worksheet.Cells[1, 5].Style.Font.Bold = true;
                worksheet.Cells[1, 5].Value = "Unit Number City";

                worksheet.Cells[1, 6].Style.Font.Bold = true;
                worksheet.Cells[1, 6].Value = "Legal Entity";

                worksheet.Cells[1, 7].Style.Font.Bold = true;
                worksheet.Cells[1, 7].Value = "Ship to State";

                worksheet.Cells[1, 8].Style.Font.Bold = true;
                worksheet.Cells[1, 8].Value = "Ship to Zip Code";

                worksheet.Cells[1, 9].Style.Font.Bold = true;
                worksheet.Cells[1, 9].Value = "Ship to City";

                worksheet.Cells[1, 10].Style.Font.Bold = true;
                worksheet.Cells[1, 10].Value = "GlAccount";

                worksheet.Cells[1, 11].Style.Font.Bold = true;
                worksheet.Cells[1, 11].Value = "Amount";

                worksheet.Cells[1, 12].Style.Font.Bold = true;
                worksheet.Cells[1, 12].Value = "OrderId";

                worksheet.Cells[1, 13].Style.Font.Bold = true;
                worksheet.Cells[1, 13].Value = "Product Name";

                worksheet.Cells[1, 14].Style.Font.Bold = true;
                worksheet.Cells[1, 14].Value = "Payment Type";

                worksheet.Cells[1, 15].Style.Font.Bold = true;
                worksheet.Cells[1, 15].Value = "Vertex Tax Area Id";

                worksheet.Cells[1, 16].Style.Font.Bold = true;
                worksheet.Cells[1, 16].Value = "Payment Date (UTC)";

                if (orders != null)
                    rowCount = orders.Count();
                else
                    throw new ArgumentNullException("orders");
                int count = 1;
                foreach (var data in orders)
                {

                    worksheet.Cells[count + 1, 1].Value = data.SiteExtKey;
                    worksheet.Cells[count + 1, 2].Value = data.SiteName;
                    worksheet.Cells[count + 1, 3].Value = data.StoreStateProvince;
                    worksheet.Cells[count + 1, 4].Value = data.StoreZipCode;
                    worksheet.Cells[count + 1, 5].Value = data.StoreCity;
                    worksheet.Cells[count + 1, 6].Value = data.LegalEntity;
                    worksheet.Cells[count + 1, 7].Value = data.ShiptoState;
                    worksheet.Cells[count + 1, 8].Value = data.ShipToZipCode;
                    worksheet.Cells[count + 1, 9].Value = data.ShippedToCity;
                    worksheet.Cells[count + 1, 10].Value = data.GLCode;
                    worksheet.Cells[count + 1, 11].Value = data.Amount;
                    worksheet.Cells[count + 1, 11].Style.Numberformat.Format = "$ #,###,###.00";
                    worksheet.Cells[count + 1, 12].Value = data.OrderId;
                    worksheet.Cells[count + 1, 13].Value = data.ProductName;
                    worksheet.Cells[count + 1, 14].Value = data.PaymentType;
                    worksheet.Cells[count + 1, 15].Value = data.VertexTaxAreaId;
                    worksheet.Cells[count + 1, 16].Value = data.paymentDate;
                    worksheet.Cells[count + 1, 16].Style.Numberformat.Format = "mm/dd/yyyy hh:mm:ss";
                    count++;
                }

                xlPackage.Save();
            }

        }

        public void ExportJournalPaymentReportPerStoretoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, int storeid = 0)
        {
            try
            {
                var newFile = new FileInfo(filePath);
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(newFile))
                {
                    int rowCount = 0;
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    var worksheet = storeid == decimal.Zero ? xlPackage.Workbook.Worksheets.Add("ReportAllUnits") : xlPackage.Workbook.Worksheets.Add("ReportsPerStore");

                    //worksheet.Cells[1, 1].Style.Font.Bold = true;
                    //worksheet.Cells[1, 1].Value = "IMPORTANT CHANGE";
                    //worksheet.Cells[1, 2].Value = "SodexoMyWay sales are now automatically reported.These reports are for reference purposes only.DO NOT self - report SodexoMyWay sales.";


                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Value = "Unit # ";
                    worksheet.Cells[1, 2].Value = storeName;

                    startDate = startDate.HasValue ? startDate : DateTime.Today;
                    startDate = Convert.ToDateTime(startDate);

                    endDate = endDate.HasValue ? endDate : DateTime.Today;
                    endDate = Convert.ToDateTime(endDate);

                    worksheet.Cells[2, 1].Style.Font.Bold = true;
                    worksheet.Cells[2, 1].Value = "Report Date: ";
                    worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                    string reportRunDate;

                    string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                    string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                    reportRunDate = reportStartDate + " - " + reportEndDate;

                    worksheet.Cells[3, 1].Style.Font.Bold = true;
                    worksheet.Cells[3, 1].Value = reportName;
                    worksheet.Cells[3, 2].Value = reportRunDate;

                    worksheet.Cells[5, 1].Style.Font.Bold = true;
                    worksheet.Cells[5, 1].Value = "Unit Number";

                    worksheet.Cells[5, 2].Style.Font.Bold = true;
                    worksheet.Cells[5, 2].Value = "Unit Name";

                    worksheet.Cells[5, 3].Style.Font.Bold = true;
                    worksheet.Cells[5, 3].Value = "Unit State";

                    worksheet.Cells[5, 4].Style.Font.Bold = true;
                    worksheet.Cells[5, 4].Value = "Unit Zip";

                    worksheet.Cells[5, 5].Style.Font.Bold = true;
                    worksheet.Cells[5, 5].Value = "Unit City";

                    worksheet.Cells[5, 6].Style.Font.Bold = true;
                    worksheet.Cells[5, 6].Value = "Legal Entity";


                    worksheet.Cells[5, 7].Style.Font.Bold = true;
                    worksheet.Cells[5, 7].Value = "Ship From State";


                    worksheet.Cells[5, 8].Style.Font.Bold = true;
                    worksheet.Cells[5, 8].Value = "Ship From Zip";

                    worksheet.Cells[5, 9].Style.Font.Bold = true;
                    worksheet.Cells[5, 9].Value = "Ship From City";

                    worksheet.Cells[5, 10].Style.Font.Bold = true;
                    worksheet.Cells[5, 10].Value = "Ship to State";

                    worksheet.Cells[5, 11].Style.Font.Bold = true;
                    worksheet.Cells[5, 11].Value = "Ship to Zip";

                    worksheet.Cells[5, 12].Style.Font.Bold = true;
                    worksheet.Cells[5, 12].Value = "Ship to City";

                    worksheet.Cells[5, 13].Style.Font.Bold = true;
                    worksheet.Cells[5, 13].Value = "GlAccount";

                    worksheet.Cells[5, 14].Style.Font.Bold = true;
                    worksheet.Cells[5, 14].Value = "Amount";

                    worksheet.Cells[5, 15].Style.Font.Bold = true;
                    worksheet.Cells[5, 15].Value = "OrderId";

                    worksheet.Cells[5, 16].Style.Font.Bold = true;
                    worksheet.Cells[5, 16].Value = "Product Name";

                    worksheet.Cells[5, 17].Style.Font.Bold = true;
                    worksheet.Cells[5, 17].Value = "Payment Type";

                    worksheet.Cells[5, 18].Style.Font.Bold = true;
                    worksheet.Cells[5, 18].Value = "Vertex Tax Area Id";

                    worksheet.Cells[5, 19].Style.Font.Bold = true;
                    worksheet.Cells[5, 19].Value = "Payment Date (UTC)";


                    if (orders != null)
                        rowCount = orders.Count();
                    else
                        throw new ArgumentNullException("orders");
                    int count = 5;
                    Dictionary<string, decimal> cardTypePriceList = new Dictionary<string, decimal>();

                    var distinctId = orders.Select(x => x.OrderId).Distinct().ToList();

                    Dictionary<string, decimal> glcodeWithAmountList = new Dictionary<string, decimal>();

                    foreach (var data in orders)
                    {
                        if (data.Amount != null)
                        {
                            if (glcodeWithAmountList.ContainsKey(data.GLCode))
                            {
                                glcodeWithAmountList[data.GLCode] += Convert.ToDecimal(data.Amount);
                            }
                            else
                            {
                                glcodeWithAmountList.Add(data.GLCode, Convert.ToDecimal(data.Amount));
                            }

                        }
                        var storedetails = _storeService.GetStoreById(data.storeid);
                        var StoreStateAbb = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(storedetails.CompanyStateProvinceId)).Abbreviation;


                        worksheet.Cells[count + 1, 1].Value = data.SiteExtKey;
                        worksheet.Cells[count + 1, 2].Value = storedetails.Name;
                        worksheet.Cells[count + 1, 3].Value = StoreStateAbb;
                        worksheet.Cells[count + 1, 4].Value = storedetails.CompanyZipPostalCode;
                        worksheet.Cells[count + 1, 5].Value = storedetails.CompanyCity;
                        worksheet.Cells[count + 1, 6].Value = data.LegalEntity;

                        //SHIP FROM SECTION
                        worksheet.Cells[count + 1, 7].Value = data.StoreStateProvince;
                        worksheet.Cells[count + 1, 8].Value = data.StoreZipCode;
                        worksheet.Cells[count + 1, 9].Value = data.StoreCity;
                        //SHIP TO SECTION
                        worksheet.Cells[count + 1, 10].Value = data.ShiptoState;
                        worksheet.Cells[count + 1, 11].Value = data.ShipToZipCode;
                        worksheet.Cells[count + 1, 12].Value = data.ShippedToCity;

                        worksheet.Cells[count + 1, 13].Value = data.GLCode;
                        worksheet.Cells[count + 1, 14].Value = data.Amount;
                        worksheet.Cells[count + 1, 14].Style.Numberformat.Format = "$ #,###,###.00";
                        worksheet.Cells[count + 1, 15].Value = data.OrderId;
                        worksheet.Cells[count + 1, 16].Value = data.ProductName;
                        worksheet.Cells[count + 1, 17].Value = data.PaymentType;
                        worksheet.Cells[count + 1, 18].Value = data.VertexTaxAreaId;
                        worksheet.Cells[count + 1, 19].Value = data.paymentDate;
                        worksheet.Cells[count + 1, 19].Style.Numberformat.Format = "mm/dd/yyyy hh:mm:ss";
                        count++;
                    }

                    var cardType = "";

                    for (int i = 0; i < distinctId.Count; i++)
                    {
                        var orderDetails = _orderService.GetOrderById(distinctId[i]);

                        var cardTypeFromFirstData = orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType") == null ? string.Empty : orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType").ToLower(); //grab cardType for each order
                        if (cardTypeFromFirstData.ToLower() == "american express")
                        {
                            cardTypeFromFirstData = "amex";
                        }

                        if (cardTypeFromFirstData != null)
                        {
                            cardType = cardTypeFromFirstData.ToLower();
                        }
                        var Filteredorders = orders.Where(x => x.OrderId == distinctId[i]).ToList();
                        foreach (var itemDetails in Filteredorders)
                        {
                            if (cardTypePriceList.ContainsKey(cardType))
                            {
                                cardTypePriceList[cardType] += Convert.ToDecimal(itemDetails.Amount);
                                //cardTypePriceList[cardType] += product.PriceInclTax;
                            }
                            else
                            {
                                cardTypePriceList.Add(cardType, Convert.ToDecimal(itemDetails.Amount));
                                //cardTypePriceList.Add(cardType, product.PriceInclTax);
                            }
                        }
                    }

                    worksheet.Cells[count + 2, 6].Style.Font.Bold = true;
                    worksheet.Cells[count + 2, 6].Value = "Total by Card Type";
                    int cardTypeCell = count + 3;

                    foreach (KeyValuePair<string, decimal> pair in cardTypePriceList)
                    {
                        worksheet.Cells[cardTypeCell, 6].Value = pair.Key;
                        worksheet.Cells[cardTypeCell, 7].Value = pair.Value;
                        worksheet.Cells[cardTypeCell, 7].Style.Numberformat.Format = "$ #,###,###.00";
                        cardTypeCell++;
                    }

                    worksheet.Cells[count + 2, 10].Style.Font.Bold = true;
                    worksheet.Cells[count + 2, 10].Value = "Total by GL";
                    int totalGlcell = count + 3;
                    foreach (KeyValuePair<string, decimal> pair in glcodeWithAmountList)
                    {
                        worksheet.Cells[totalGlcell, 10].Value = pair.Key;
                        worksheet.Cells[totalGlcell, 11].Value = pair.Value;
                        worksheet.Cells[totalGlcell, 11].Style.Numberformat.Format = "$ #,###,###.00";
                        totalGlcell++;
                    }

                    xlPackage.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.ToString());
            }

        }


        public void ExportJournalPaymentReportPerAndAllStore(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, int storeId = 0)
        {
            try
            {
                var newFile = new FileInfo(filePath);
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(newFile))
                {
                    int rowCount = 0;
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    var worksheet = storeId == decimal.Zero ? xlPackage.Workbook.Worksheets.Add("ReportAllUnits") : xlPackage.Workbook.Worksheets.Add("ReportsPerStore");

                    if (storeId != decimal.Zero)
                    {
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1].Value = storeId == decimal.Zero ? string.Empty : "IMPORTANT CHANGE:";
                        worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                        worksheet.Cells[1, 2].Value = storeId == decimal.Zero ? string.Empty : "SodexoMyWay sales are now automatically reported.These reports are for reference purposes only.DO NOT self - report SodexoMyWay sales.";

                        worksheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                    }

                    worksheet.Cells[storeId != decimal.Zero ? 2 : 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 2 : 1, 1].Value = "Unit # ";
                    worksheet.Cells[storeId != decimal.Zero ? 2 : 1, 2].Value = storeName;

                    startDate = startDate.HasValue ? startDate : DateTime.Today;
                    startDate = Convert.ToDateTime(startDate);

                    endDate = endDate.HasValue ? endDate : DateTime.Today;
                    endDate = Convert.ToDateTime(endDate);

                    worksheet.Cells[storeId != decimal.Zero ? 3 : 2, 1].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 3 : 2, 1].Value = "Report Date: ";
                    worksheet.Cells[storeId != decimal.Zero ? 3 : 2, 2].Value = String.Format("{0:d}", DateTime.Today);

                    string reportRunDate;

                    string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                    string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                    reportRunDate = reportStartDate + " - " + reportEndDate;

                    worksheet.Cells[storeId != decimal.Zero ? 4 : 3, 1].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 4 : 3, 1].Value = reportName;
                    worksheet.Cells[storeId != decimal.Zero ? 4 : 3, 2].Value = reportRunDate;

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 1].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 1].Value = "Unit Number";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 2].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 2].Value = "Unit Name";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 3].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 3].Value = "Unit State";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 4].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 4].Value = "Unit Zip";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 5].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 5].Value = "Unit City";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 6].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 6].Value = "Legal Entity";


                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 7].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 7].Value = "Ship From State";


                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 8].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 8].Value = "Ship From Zip";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 9].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 9].Value = "Ship From City";


                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 10].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 10].Value = "Ship to State";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 11].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 11].Value = "Ship to Zip";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 12].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 12].Value = "Ship to City";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 13].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 13].Value = "GlAccount";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 14].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 14].Value = "Amount";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 15].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 15].Value = "OrderId";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 16].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 16].Value = "Product Name";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 17].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 17].Value = "Payment Type";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 18].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 18].Value = "Vertex Tax Area Id";

                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 19].Style.Font.Bold = true;
                    worksheet.Cells[storeId != decimal.Zero ? 5 : 5, 19].Value = "Transaction Date (UTC)";

                    if (orders != null)
                        rowCount = orders.Count();
                    else
                        throw new ArgumentNullException("orders");
                    int count = 5;
                    Dictionary<string, decimal> cardTypePriceList = new Dictionary<string, decimal>();

                    var distinctId = orders.Select(x => x.OrderId).Distinct().ToList();

                    Dictionary<string, decimal> glcodeWithAmountList = new Dictionary<string, decimal>();

                    foreach (var data in orders)
                    {

                        var orderDetails = _orderService.GetOrderById(data.OrderId);

                        if (data.Amount != null)
                        {
                            if (glcodeWithAmountList.ContainsKey(data.GLCode))
                            {
                                glcodeWithAmountList[data.GLCode] += Convert.ToDecimal(data.Amount);
                            }
                            else
                            {
                                glcodeWithAmountList.Add(data.GLCode, Convert.ToDecimal(data.Amount));
                            }

                        }

                        var cardTypeFromFirstData = orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType") == null ? string.Empty : orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType").ToLower(); //grab cardType for each order

                        if (cardTypeFromFirstData.ToLower() == "american express")
                        {
                            data.PaymentType = "amex";
                        }

                        if (cardTypeFromFirstData != null)
                        {
                            data.PaymentType = cardTypeFromFirstData.ToLower();
                        }

                        if (orderDetails.PaymentMethodSystemName == "Payments.PayPalExpressCheckout")
                            data.PaymentType = "Paypal";

                        if (orderDetails.PaymentMethodSystemName == "TroveDigitalPayments")
                            data.PaymentType = _encryptionService.DecryptText(orderDetails.CardType);


                        worksheet.Cells[count + 1, 1].Value = data.SiteExtKey;
                        worksheet.Cells[count + 1, 2].Value = data.storeName;
                        worksheet.Cells[count + 1, 3].Value = data.storeSate;
                        worksheet.Cells[count + 1, 4].Value = data.storeZip;
                        worksheet.Cells[count + 1, 5].Value = data.StoreCity;
                        worksheet.Cells[count + 1, 6].Value = data.LegalEntity;

                        //SHIP FROM SECTION
                        worksheet.Cells[count + 1, 7].Value = data.ShipFromState;
                        worksheet.Cells[count + 1, 8].Value = data.ShipFromZipCode;
                        worksheet.Cells[count + 1, 9].Value = data.ShipFromCity;
                        //SHIP TO SECTION
                        worksheet.Cells[count + 1, 10].Value = data.ShiptoState;
                        worksheet.Cells[count + 1, 11].Value = data.ShipToZipCode;
                        worksheet.Cells[count + 1, 12].Value = data.ShippedToCity;

                        worksheet.Cells[count + 1, 13].Value = data.GLCode;
                        worksheet.Cells[count + 1, 14].Value = data.Amount;
                        worksheet.Cells[count + 1, 14].Style.Numberformat.Format = "$ #,###,###.00";
                        worksheet.Cells[count + 1, 15].Value = data.OrderId;
                        worksheet.Cells[count + 1, 16].Value = data.ProductName;
                        worksheet.Cells[count + 1, 17].Value = data.PaymentType;
                        worksheet.Cells[count + 1, 18].Value = data.VertexTaxAreaId;
                        var dateToPrint = data.paymentDate;
                        if (data.GLCode == "64701000")
                        {
                            // This is a bonus! Positive amounts are refunds, negatives are purchases. 
                            if (data.Amount >= 0 && data.DateOfRefund != null)
                            {
                                dateToPrint = data.DateOfRefund;
                            }
                            else
                            {
                                dateToPrint = data.paymentDate;
                            }
                        }
                        else
                        {
                            if (data.DateOfRefund == null || data.Amount >= 0)
                            {
                                dateToPrint = data.paymentDate;
                            }
                            else
                            {
                                dateToPrint = data.DateOfRefund;
                            }
                        }
                        worksheet.Cells[count + 1, 19].Value = dateToPrint;
                        worksheet.Cells[count + 1, 19].Style.Numberformat.Format = "mm/dd/yyyy hh:mm:ss";
                        count++;
                    }

                    var cardType = "";

                    for (int i = 0; i < distinctId.Count; i++)
                    {
                        var orderDetails = _orderService.GetOrderById(distinctId[i]);

                        var cardTypeFromFirstData = orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType") == null ? string.Empty : orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType").ToLower(); //grab cardType for each order
                        if (cardTypeFromFirstData.ToLower() == "american express")
                        {
                            cardTypeFromFirstData = "amex";
                        }

                        if (cardTypeFromFirstData != null)
                        {
                            cardType = cardTypeFromFirstData.ToLower();
                        }
                        if (orderDetails.PaymentMethodSystemName == "TroveDigitalPayments")
                            cardType = _encryptionService.DecryptText(orderDetails.CardType);

                        var Filteredorders = orders.Where(x => x.OrderId == distinctId[i]).ToList();
                        foreach (var itemDetails in Filteredorders)
                        {
                            if (cardTypePriceList.ContainsKey(cardType))
                            {
                                cardTypePriceList[cardType] += Convert.ToDecimal(itemDetails.Amount);
                                //cardTypePriceList[cardType] += product.PriceInclTax;
                            }
                            else
                            {
                                cardTypePriceList.Add(cardType, Convert.ToDecimal(itemDetails.Amount));
                                //cardTypePriceList.Add(cardType, product.PriceInclTax);
                            }
                        }
                    }

                    worksheet.Cells[count + 2, 6].Style.Font.Bold = true;
                    worksheet.Cells[count + 2, 6].Value = "Total by Card Type";
                    int cardTypeCell = count + 3;

                    foreach (KeyValuePair<string, decimal> pair in cardTypePriceList)
                    {
                        worksheet.Cells[cardTypeCell, 6].Value = pair.Key;
                        worksheet.Cells[cardTypeCell, 7].Value = pair.Value;
                        worksheet.Cells[cardTypeCell, 7].Style.Numberformat.Format = "$ #,###,###.00";
                        cardTypeCell++;
                    }

                    worksheet.Cells[count + 2, 10].Style.Font.Bold = true;
                    worksheet.Cells[count + 2, 10].Value = "Total by GL";
                    int totalGlcell = count + 3;
                    foreach (KeyValuePair<string, decimal> pair in glcodeWithAmountList)
                    {
                        worksheet.Cells[totalGlcell, 10].Value = pair.Key;
                        worksheet.Cells[totalGlcell, 11].Value = pair.Value;
                        worksheet.Cells[totalGlcell, 11].Style.Numberformat.Format = "$ #,###,###.00";
                        totalGlcell++;
                    }

                    xlPackage.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.ToString());
            }

        }

        public void ExportJournalDEliveryReporttoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                int rowCount = 0;
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                var worksheet = xlPackage.Workbook.Worksheets.Add("JournalDeliveryReports");

                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "Unit # ";
                worksheet.Cells[1, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Report Date: ";
                worksheet.Cells[2, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[5, 1].Style.Font.Bold = true;
                worksheet.Cells[5, 1].Value = "Unit Number";

                worksheet.Cells[5, 2].Style.Font.Bold = true;
                worksheet.Cells[5, 2].Value = "Unit Name";

                worksheet.Cells[5, 3].Style.Font.Bold = true;
                worksheet.Cells[5, 3].Value = "OrderId";

                worksheet.Cells[5, 4].Style.Font.Bold = true;
                worksheet.Cells[5, 4].Value = "Product Name";

                worksheet.Cells[5, 5].Style.Font.Bold = true;
                worksheet.Cells[5, 5].Value = "Fullfillment Date";

                worksheet.Cells[5, 6].Style.Font.Bold = true;
                worksheet.Cells[5, 6].Value = "Gl Account";

                worksheet.Cells[5, 7].Style.Font.Bold = true;
                worksheet.Cells[5, 7].Value = "Amount";


                if (orders != null)
                    rowCount = orders.Count();
                else
                    throw new ArgumentNullException("orders");
                int count = 5;
                foreach (var data in orders)
                {
                    if (string.IsNullOrEmpty(data.storeName))
                    {
                        data.storeName = _storeService.GetStoreById(data.storeid).Name;
                    }

                    worksheet.Cells[count + 1, 1].Value = data.SiteExtKey;
                    worksheet.Cells[count + 1, 2].Value = data.storeName;
                    worksheet.Cells[count + 1, 3].Value = data.OrderId;
                    worksheet.Cells[count + 1, 4].Value = data.ProductName;
                    worksheet.Cells[count + 1, 5].Value = data.RequestedFulfillmentDate;
                    worksheet.Cells[count + 1, 5].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[count + 1, 6].Value = data.GLCode;
                    worksheet.Cells[count + 1, 7].Value = data.Amount;
                    worksheet.Cells[count + 1, 7].Style.Numberformat.Format = "$ #,###,###.00";

                    count++;

                }

                xlPackage.Save();
            }

        }

        public void ExportJournalDEliveryReportPerStoretoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                int rowCount = 0;
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                var worksheet = xlPackage.Workbook.Worksheets.Add("DeliveryReportsPerStore");

                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "IMPORTANT CHANGE:";
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                worksheet.Cells[1, 2].Value = "SodexoMyWay sales are now automatically reported.These reports are for reference purposes only.DO NOT self - report SodexoMyWay sales.";
                worksheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = "Unit # ";
                worksheet.Cells[2, 2].Value = storeName;

                startDate = startDate.HasValue ? startDate : DateTime.Today;
                startDate = Convert.ToDateTime(startDate);

                endDate = endDate.HasValue ? endDate : DateTime.Today;
                endDate = Convert.ToDateTime(endDate);

                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Value = "Report Date: ";
                worksheet.Cells[3, 2].Value = String.Format("{0:d}", DateTime.Today);

                string reportRunDate;

                string reportStartDate = "";//startDate.HasValue ? String.Format("{0:d}", startDate) : String.Format("{0:d}", DateTime.Today);
                string reportEndDate = "";//endDate.HasValue ? String.Format("{0:d}", endDate) : String.Format("{0:d}", DateTime.Today);

                reportRunDate = reportStartDate + " - " + reportEndDate;

                worksheet.Cells[4, 1].Style.Font.Bold = true;
                worksheet.Cells[4, 1].Value = reportName;
                worksheet.Cells[4, 2].Value = reportRunDate;


                worksheet.Cells[5, 1].Style.Font.Bold = true;
                worksheet.Cells[5, 1].Value = "Unit Number";

                worksheet.Cells[5, 2].Style.Font.Bold = true;
                worksheet.Cells[5, 2].Value = "Unit Name";

                worksheet.Cells[5, 3].Style.Font.Bold = true;
                worksheet.Cells[5, 3].Value = "OrderId";

                worksheet.Cells[5, 4].Style.Font.Bold = true;
                worksheet.Cells[5, 4].Value = "Product Name";

                worksheet.Cells[5, 5].Style.Font.Bold = true;
                worksheet.Cells[5, 5].Value = "Fullfillment Date";

                worksheet.Cells[5, 6].Style.Font.Bold = true;
                worksheet.Cells[5, 6].Value = "Gl Account";

                worksheet.Cells[5, 7].Style.Font.Bold = true;
                worksheet.Cells[5, 7].Value = "Amount";



                if (orders != null)
                    rowCount = orders.Count();
                else
                    throw new ArgumentNullException("orders");
                int count = 5;

                Dictionary<string, decimal> cardTypePriceList = new Dictionary<string, decimal>();
                var distinctId = orders.Select(x => x.OrderId).Distinct().ToList();
                Dictionary<string, decimal> glcodeWithAmountList = new Dictionary<string, decimal>();

                foreach (var data in orders)
                {
                    if (glcodeWithAmountList.ContainsKey(data.GLCode))
                    {
                        glcodeWithAmountList[data.GLCode] += (decimal)data.Amount;
                    }
                    else
                    {
                        glcodeWithAmountList.Add(data.GLCode, (decimal)data.Amount);

                    }


                    worksheet.Cells[count + 1, 1].Value = data.SiteExtKey;
                    worksheet.Cells[count + 1, 2].Value = data.storeName;
                    worksheet.Cells[count + 1, 3].Value = data.OrderId;
                    worksheet.Cells[count + 1, 4].Value = data.ProductName;
                    worksheet.Cells[count + 1, 5].Value = data.RequestedFulfillmentDate;
                    worksheet.Cells[count + 1, 5].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[count + 1, 6].Value = data.GLCode;
                    worksheet.Cells[count + 1, 7].Value = data.Amount;
                    worksheet.Cells[count + 1, 7].Style.Numberformat.Format = "$ #,###,###.00";

                    //worksheet.Cells[count + 1, 1].Value = data.SiteExtKey;
                    //worksheet.Cells[count + 1, 2].Value = data.OrderId;
                    //worksheet.Cells[count + 1, 3].Value = data.ProductName;
                    //worksheet.Cells[count + 1, 4].Value = data.RequestedFulfillmentDate;
                    //worksheet.Cells[count + 1, 4].Style.Numberformat.Format = "mm/dd/yyyy";
                    //worksheet.Cells[count + 1, 5].Value = data.GLCode;
                    //worksheet.Cells[count + 1, 6].Value = data.Amount;
                    //worksheet.Cells[count + 1, 6].Style.Numberformat.Format = "$ #,###,###.00";

                    count++;

                }

                var cardType = "";

                for (int i = 0; i < distinctId.Count; i++)
                {
                    var orderDetails = _orderService.GetOrderById(distinctId[i]);

                    var cardTypeFromFirstData = orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType") == null ? string.Empty : orderDetails.GetAttribute<string>("BitShift.Payments.FirstData.TransactionCardType").ToLower(); //grab cardType for each order
                    if (cardTypeFromFirstData.ToLower() == "american express")
                    {
                        cardTypeFromFirstData = "amex";
                    }

                    if (cardTypeFromFirstData != null)
                    {
                        cardType = cardTypeFromFirstData.ToLower();
                    }
                    var Filteredorders = orders.Where(x => x.OrderId == distinctId[i]).ToList();
                    foreach (var itemDetails in Filteredorders)
                    {
                        if (cardTypePriceList.ContainsKey(cardType))
                        {
                            //if (product.IsShipping)
                            cardTypePriceList[cardType] += Convert.ToDecimal(itemDetails.Amount);
                            //if (product.IsDeliveryPickUp)
                            //    cardTypePriceList[cardType] += Convert.ToDecimal(product.PriceExclTax) + Convert.ToDecimal(product.DeliveryPickupFee);

                        }
                        else
                        {
                            //if (product.IsShipping)
                            cardTypePriceList.Add(cardType, Convert.ToDecimal(itemDetails.Amount)) /*+ Convert.ToDecimal(product.ShippingFee))*/;
                            // if (product.IsDeliveryPickUp)
                            // cardTypePriceList.Add(cardType, Convert.ToDecimal(product.PriceExclTax) + Convert.ToDecimal(product.DeliveryPickupFee));
                        }
                    }

                    //foreach (var product in orderDetails.OrderItems)
                    //{
                    //    if (cardTypePriceList.ContainsKey(cardType))
                    //    {
                    //        if (product.IsShipping)
                    //            cardTypePriceList[cardType] += Convert.ToDecimal(product.PriceExclTax) + Convert.ToDecimal(product.ShippingFee);
                    //        if (product.IsDeliveryPickUp)
                    //            cardTypePriceList[cardType] += Convert.ToDecimal(product.PriceExclTax) + Convert.ToDecimal(product.DeliveryPickupFee);

                    //    }
                    //    else
                    //    {
                    //        if (product.IsShipping)
                    //            cardTypePriceList.Add(cardType, Convert.ToDecimal(product.PriceExclTax) + Convert.ToDecimal(product.ShippingFee));
                    //        if (product.IsDeliveryPickUp)
                    //            cardTypePriceList.Add(cardType, Convert.ToDecimal(product.PriceExclTax) + Convert.ToDecimal(product.DeliveryPickupFee));

                    //    }
                    //}
                }

                worksheet.Cells[count + 2, 3].Style.Font.Bold = true;
                worksheet.Cells[count + 2, 3].Value = "Total by Card Type";
                int cardTypeCell = count + 3;

                foreach (KeyValuePair<string, decimal> pair in cardTypePriceList)
                {
                    worksheet.Cells[cardTypeCell, 3].Value = pair.Key;
                    worksheet.Cells[cardTypeCell, 4].Value = pair.Value;
                    worksheet.Cells[cardTypeCell, 4].Style.Numberformat.Format = "$ #,###,###.00";
                    cardTypeCell++;
                }

                worksheet.Cells[count + 2, 6].Style.Font.Bold = true;
                worksheet.Cells[count + 2, 6].Value = "Total by GL";
                int totalGlcell = count + 3;
                foreach (KeyValuePair<string, decimal> pair in glcodeWithAmountList)
                {
                    worksheet.Cells[totalGlcell, 6].Value = pair.Key;
                    worksheet.Cells[totalGlcell, 7].Value = pair.Value;
                    worksheet.Cells[totalGlcell, 7].Style.Numberformat.Format = "$ #,###,###.00";
                    totalGlcell++;
                }

                xlPackage.Save();
            }

        }
        #endregion

        public DataTable ToDataTable(List<KitchenProduction> kitchenProductionDataList, string name)
        {
            var item = kitchenProductionDataList.Max(x => x.maxAttributes);

            DataTable dt = new DataTable(name);
            dt.Columns.Add("Order Date");
            dt.Columns.Add("Order Time");
            dt.Columns.Add("Orderid");
            dt.Columns.Add("Category");
            dt.Columns.Add("ProductName");
            dt.Columns.Add("Pickup Date");
            dt.Columns.Add("Pickup Location");
            dt.Columns.Add("Customer Name");
            dt.Columns.Add("Phone Number");
            dt.Columns.Add("Email");

            for (int i = 0; i < item; i++)
            {
                dt.Columns.Add("Attributes:" + (i + 1));
            }

            foreach (var items in kitchenProductionDataList)
            {
                DataRow dr = dt.NewRow();
                dr["Order Date"] = String.Format("{0:MM/dd/yyyy}", items.createdOnDate);
                dr["Order Time"] = String.Format("{0:HH:mm:ss tt}", items.createdOnDate);
                dr["Orderid"] = items.orderid;
                dr["Category"] = items.Categories;
                dr["ProductName"] = items.productName;
                dr["Pickup Date"] = items.pickupDate;
                dr["Pickup Location"] = items.pickupLocation;
                dr["Customer Name"] = items.userName;
                dr["Phone Number"] = items.phone;
                dr["Email"] = items.email;

                for (int j = 0; j < items.attributesarr.Count(); j++)
                {
                    dr["Attributes:" + (j + 1)] = items.attributesarr[j];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion
    }
}
