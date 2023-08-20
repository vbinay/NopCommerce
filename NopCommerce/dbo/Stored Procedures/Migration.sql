
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Migration]
AS
BEGIN
	SET NOCOUNT ON;
	
	--- Store
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Store]'
	BEGIN

		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Store] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Store] NOCHECK CONSTRAINT ALL

		INSERT INTO [nopcommerce_prod].[dbo].[Store]
			   ([Id],
			   [Name] ,
			   [Url] ,
			   [SslEnabled] ,
			   [SecureUrl] ,
			   [Hosts] ,
			   [DefaultLanguageId] ,
			   [DisplayOrder] ,
			   [CompanyName] ,
			   [CompanyAddress] ,
			   [CompanyPhoneNumber],
			   [CompanyVat])
		SELECT [smwbluedot_prod].dbo.SourceSite.Id,
				[smwbluedot_prod].dbo.SourceSiteSettings.StoreName, 
				[nopcommerce_prod].dbo.updateStoreURL([smwbluedot_prod].dbo.SourceSiteSettings.StoreUrl), 
				0, 
				[nopcommerce_prod].dbo.updateStoreURL([smwbluedot_prod].dbo.SourceSiteSettings.StoreUrl), 
				[nopcommerce_prod].dbo.updateStoreURL([nopcommerce_prod].dbo.gethostname([smwbluedot_prod].dbo.SourceSiteSettings.StoreUrl)),
				0, 
				0,
				[smwbluedot_prod].dbo.SourceSite.Name,
				'',
				'',
				''
			FROM [smwbluedot_prod].dbo.SourceSite INNER JOIN
				[smwbluedot_prod].dbo.SourceSiteSettings ON [smwbluedot_prod].dbo.SourceSite.Id = [smwbluedot_prod].dbo.SourceSiteSettings.SourceSiteId

		ALTER TABLE [nopcommerce_prod].[dbo].[Store] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Store]	OFF
		
		SELECT 'Stores', COUNT(*) FROM [nopcommerce_prod].[dbo].[Store]
	END

	--- Store > StoreMapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[StoreMapping]'
	
	--- Address 
	ALTER TABLE [nopcommerce_prod].[dbo].[Customer] NOCHECK CONSTRAINT ALL
	ALTER TABLE [nopcommerce_prod].[dbo].[Order] NOCHECK CONSTRAINT ALL
	DELETE FROM [nopcommerce_prod].[dbo].[Address] WHERE Id > 1
	ALTER TABLE [nopcommerce_prod].[dbo].[Order] CHECK CONSTRAINT ALL
	ALTER TABLE [nopcommerce_prod].[dbo].[Customer] CHECK CONSTRAINT ALL
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Address] NOCHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Address] ON
		
		INSERT INTO [nopcommerce_prod].[dbo].[Address]
           ([Id]
           ,[FirstName]
           ,[LastName]
           ,[Email]
           ,[Company]
           ,[CountryId]
           ,[StateProvinceId]
           ,[City]
           ,[Address1]
           ,[Address2]
           ,[ZipPostalCode]
           ,[PhoneNumber]
           ,[FaxNumber]
           ,[CustomAttributes]
           ,[CreatedOnUtc])
     SELECT
           [Id]
           ,[FirstName]
           ,[LastName]
           ,[Email]
           ,[Company]
           ,[CountryId]
           ,[StateProvinceId]
           ,[City]
           ,[Address1]
           ,[Address2]
           ,[ZipPostalCode]
           ,[PhoneNumber]
           ,[FaxNumber]
           ,null
           ,[CreatedOnUtc]
        FROM [smwbluedot_prod].[dbo].[Address]
        WHERE Id > 1
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Address] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Address] OFF
		
		SELECT 'Address', COUNT(*) FROM [nopcommerce_prod].[dbo].[Address]
	END

	--- Address > AddressAttribute
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[AddressAttribute]'

	--- Address > AddressAttributeValue
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[AddressAttributeValue]'
	
	--- Affiliate 
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Affiliate]'
	BEGIN

		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Affiliate] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Affiliate] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Affiliate]
           ([Id]
		   ,[AddressId]
           ,[Deleted]
           ,[Active])
		SELECT
           [Id]
		   ,[AddressId]
           ,[Deleted]
           ,[Active]
        FROM [smwbluedot_prod].[dbo].[Affiliate]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Affiliate] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Affiliate] OFF
		
		SELECT 'Affiliate', COUNT(*) FROM [nopcommerce_prod].[dbo].[Affiliate]
	END

	--- BackInStockSubscription
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[BackInStockSubscription]'

	--- Campaign
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Campaign]'
	
	--- Category
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Category]'
	BEGIN
	
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Category] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Category] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Category]
		   ([Id]
		   ,[Name]
		   ,[Description]
		   ,[CategoryTemplateId]
		   ,[MetaKeywords]
		   ,[MetaDescription]
		   ,[MetaTitle]
		   ,[ParentCategoryId]
		   ,[PictureId]
		   ,[PageSize]
		   ,[AllowCustomersToSelectPageSize]
		   ,[PageSizeOptions]
		   ,[PriceRanges]
		   ,[ShowOnHomePage]
		   ,[IncludeInTopMenu]
		   ,[SubjectToAcl]
		   ,[LimitedToStores]
		   ,[Published]
		   ,[Deleted]
		   ,[DisplayOrder]
		   ,[CreatedOnUtc]
		   ,[UpdatedOnUtc]
		   ,[IsMaster])
		SELECT 
			Id
			,Name
			,[Description]
			,1
			,NULL
			,NULL
			,NULL
			,ParentCategoryId
			,PictureId
			,PageSize
			,AllowCustomersToSelectPageSize
			,PageSizeOptions
			,PriceRanges
			,ShowOnHomePage
			,1
			,0
			,0
		   ,Published
		   ,Deleted
		   ,DisplayOrder
		   ,CreatedOnUtc
		   ,UpdatedOnUtc 
		   ,1
		FROM smwbluedot_prod.dbo.Category
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Category] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Category] OFF
		
		SELECT 'Category', COUNT(*) FROM [nopcommerce_prod].[dbo].[Category]
	END
	
	--- Category > CategoryTemplate
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CategoryTemplate]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CategoryTemplate] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[CategoryTemplate] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[CategoryTemplate]
           ([Id]
           ,[Name]
           ,[ViewPath]
           ,[DisplayOrder])
		SELECT [Id]
           ,[Name]
           ,[ViewPath]
           ,[DisplayOrder]
			FROM [smwbluedot_prod].[dbo].[CategoryTemplate]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[CategoryTemplate] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CategoryTemplate] OFF
			   
		SELECT 'CategoryTemplate', COUNT(*) FROM [nopcommerce_prod].[dbo].[CategoryTemplate]
	END

	--- Category > UrlRecord
	DELETE FROM [nopcommerce_prod].[dbo].[UrlRecord] WHERE EntityName = 'Category'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[UrlRecord] NOCHECK CONSTRAINT ALL

		INSERT INTO [nopcommerce_prod].[dbo].[UrlRecord]
			   ([EntityId]
			   ,[EntityName]
			   ,[Slug]
			   ,[IsActive]
			   ,[LanguageId])
		 SELECT 
			   [Id]
			   ,'Category'
			   ,[nopcommerce_prod].[dbo].createSlug('Category', [nopcommerce_prod].[dbo].[Category].Name, 0)
			   ,[Published]
			   ,0
			   FROM [nopcommerce_prod].[dbo].[Category]
			   
		ALTER TABLE [nopcommerce_prod].[dbo].[UrlRecord] CHECK CONSTRAINT ALL

		SELECT 'UrlRecord', COUNT(*) FROM [nopcommerce_prod].[dbo].[UrlRecord]
			WHERE EntityName = 'Category'
	END
	
	--- CheckoutAttribute
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CheckoutAttribute]'
	BEGIN
		
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CheckoutAttribute] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[CheckoutAttribute] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[CheckoutAttribute]
           ([Id]
		   ,[Name]
           ,[TextPrompt]
           ,[IsRequired]
           ,[ShippableProductRequired]
           ,[IsTaxExempt]
           ,[TaxCategoryId]
           ,[AttributeControlTypeId]
           ,[DisplayOrder]
           ,[LimitedToStores]
           ,[ValidationMinLength]
           ,[ValidationMaxLength]
           ,[ValidationFileAllowedExtensions]
           ,[ValidationFileMaximumSize]
           ,[DefaultValue])
		SELECT 
			[Id]
		   ,[Name]
           ,[TextPrompt]
           ,[IsRequired]
           ,[ShippableProductRequired]
           ,[IsTaxExempt]
           ,[TaxCategoryId]
           ,[AttributeControlTypeId]
           ,[DisplayOrder]
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
		FROM smwbluedot_prod.dbo.[CheckoutAttribute]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[CheckoutAttribute] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CheckoutAttribute] OFF
			   
		SELECT 'CheckoutAttribute', COUNT(*) FROM [nopcommerce_prod].[dbo].[CheckoutAttribute]
	END
	
	--- CheckoutAttribute > CheckoutAttributeValue
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CheckoutAttributeValue]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CheckoutAttributeValue] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[CheckoutAttributeValue] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[CheckoutAttributeValue]
           ([Id]
		   ,[CheckoutAttributeId]
           ,[Name]
           ,[ColorSquaresRgb]
           ,[PriceAdjustment]
           ,[WeightAdjustment]
           ,[IsPreSelected]
           ,[DisplayOrder])
		SELECT 
			[Id]
		   ,[CheckoutAttributeId]
           ,[Name]
           ,0
           ,[PriceAdjustment]
           ,[WeightAdjustment]
           ,[IsPreSelected]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[CheckoutAttributeValue]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[CheckoutAttributeValue] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CheckoutAttributeValue] OFF
			   
		SELECT 'CheckoutAttributeValue', COUNT(*) FROM [nopcommerce_prod].[dbo].[CheckoutAttributeValue]
	END

	--- CrossSellProduct --- @TO DO: Product, Produt variant or Site Product?
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CrossSellProduct]'
	BEGIN

		ALTER TABLE [nopcommerce_prod].[dbo].[CrossSellProduct] NOCHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CrossSellProduct] ON
	
		INSERT INTO [nopcommerce_prod].[dbo].[CrossSellProduct]
           ([Id]
		   ,[ProductId1]
           ,[ProductId2])
		SELECT
           [Id]
		   ,SiteProductId1
           ,SiteProductId2
        FROM [smwbluedot_prod].[dbo].[CrossSellProduct]
        
        UPDATE cs
			SET cs.[ProductId1] = p.Id
			FROM [nopcommerce_prod].[dbo].[CrossSellProduct] cs INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON cs.ProductId1 = p.SiteProductId
		
        UPDATE cs
			SET cs.[ProductId2] = p.Id
			FROM [nopcommerce_prod].[dbo].[CrossSellProduct] cs INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON cs.ProductId2 = p.SiteProductId

		ALTER TABLE [nopcommerce_prod].[dbo].[CrossSellProduct] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CrossSellProduct] OFF
		
		SELECT 'CrossSellProduct', COUNT(*) FROM [nopcommerce_prod].[dbo].[CrossSellProduct]
	END

	--- Customer
	DELETE FROM [nopcommerce_prod].[dbo].[Customer] WHERE Id > 1
	BEGIN

		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Customer] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Customer] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Customer]
			   ([Id]
			   ,[CustomerGuid]
			   ,[Username]
			   ,[Email]
			   ,[Password]
			   ,[PasswordFormatId]
			   ,[PasswordSalt]
			   ,[AdminComment]
			   ,[IsTaxExempt]
			   ,[AffiliateId]
			   ,[VendorId]
			   ,[HasShoppingCartItems]
			   ,[Active]
			   ,[Deleted]
			   ,[IsSystemAccount]
			   ,[SystemName]
			   ,[LastIpAddress]
			   ,[CreatedOnUtc]
			   ,[LastLoginDateUtc]
			   ,[LastActivityDateUtc]
			   ,[BillingAddress_Id]
			   ,[ShippingAddress_Id])
		 SELECT
			   [Id]
			   ,[CustomerGuid]
			   ,[Username]
			   ,[nopcommerce_prod].dbo.updateEmail([Email])
			   ,'6B003EF2D2FEF9B42306BC48D88BA290C625DB52'
			   ,1
			   ,'GbMWgoM='
			   ,[AdminComment]
			   ,[IsTaxExempt]
			   ,0
			   ,0
			   ,0
			   ,[Active]
			   ,[Deleted]
			   ,[IsSystemAccount]
			   ,[SystemName]
			   ,[LastIpAddress]
			   ,[CreatedOnUtc]
			   ,[LastLoginDateUtc]
			   ,[LastActivityDateUtc]
			   ,[BillingAddress_Id]
			   ,[ShippingAddress_Id]
			FROM	
				smwbluedot_prod.dbo.Customer
			WHERE
				( BillingAddress_Id IS NOT NULL
				OR ShippingAddress_Id IS NOT NULL 
				OR Email IS NOT NULL )
				AND Id > 1
				
		ALTER TABLE [nopcommerce_prod].[dbo].[Customer] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Customer] OFF
		
		UPDATE [nopcommerce_prod].[dbo].[Customer]
			SET LimitedToStores = 0
			WHERE LimitedToStores IS NULL

		SELECT 'Customers', COUNT(*) FROM [nopcommerce_prod].[dbo].[Customer]
	END

	--- Customer -> CustomerRole
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CustomerRole]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CustomerRole] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[CustomerRole] NOCHECK CONSTRAINT ALL

		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(1,'System Administrators',0,0,1,1,'Administrators',0)
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(2,'Forum Moderators',0,0,1,1,'ForumModerators',0)
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(3,'Registered',0,0,1,1,'Registered',0)
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(4,'Guests',0,0,1,1,'Guests',0)
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(5,'Vendors',0,0,1,1,'Vendors',0)
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(6,'Global Administrators',0,0,1,0,'GlobalAdministrators',0)
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerRole](Id, Name, FreeShipping, TaxExempt, Active, IsSystemRole, SystemName, PurchasedWithProductId) 
			VALUES(7,'Store Administrators',0,0,1,0,'StoreAdministrators',0)
	
		ALTER TABLE [nopcommerce_prod].[dbo].[CustomerRole] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[CustomerRole] OFF
		
		SELECT 'CustomerRole', COUNT(*) FROM [nopcommerce_prod].[dbo].[CustomerRole]
	END

	--- Customer > Customer_CustomerRole_Mapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping]'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping]
			   ([Customer_Id]
			   ,[CustomerRole_Id])
			VALUES
			   (1,1)

		INSERT INTO [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping]
			   ([Customer_Id]
			   ,[CustomerRole_Id])
			SELECT DISTINCT
				Customer.Id, 
				CASE SMWCustomerRole.CustomerRole_Id
					WHEN 5 THEN 1
					WHEN 2 THEN 6
					WHEN 1 THEN 7
					ELSE SMWCustomerRole.CustomerRole_Id
				END
			FROM
				[nopcommerce_prod].[dbo].Customer AS Customer INNER JOIN smwbluedot_prod.dbo.Customer AS SMWCustomer
					ON Customer.Id = SMWCustomer.Id
				INNER JOIN smwbluedot_prod.dbo.Customer_CustomerRole_Mapping AS SMWCustomerRole
					ON SMWCustomer.Id = SMWCustomerRole.Customer_Id		
					
		INSERT INTO [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping]
			   ([Customer_Id]
			   ,[CustomerRole_Id])
		SELECT DISTINCT 
			Parent.[Customer_Id],
			3
			FROM [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping] Parent
			WHERE EXISTS (
				SELECT *
				FROM [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping] Child
				WHERE Parent.Customer_Id = Child.Customer_Id
					AND Child.CustomerRole_Id IN (1,6,7)
					AND Child.Customer_Id NOT IN (
						SELECT GrandChild.Customer_Id
						FROM [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping] GrandChild
						WHERE GrandChild.CustomerRole_Id = 3
					)
			)
					
		ALTER TABLE [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping] CHECK CONSTRAINT ALL
			   
		SELECT 'Customer_CustomerRole_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Customer_CustomerRole_Mapping]
	END

	--- Customer > CustomerAddresses
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CustomerAddresses]'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[CustomerAddresses] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[CustomerAddresses]
           ([Customer_Id]
           ,[Address_Id])
		SELECT
           [Customer_Id]
           ,[Address_Id]
			FROM [smwbluedot_prod].[dbo].[CustomerAddresses]
					
		ALTER TABLE [nopcommerce_prod].[dbo].[CustomerAddresses] CHECK CONSTRAINT ALL
			   
		SELECT 'CustomerAddresses', COUNT(*) FROM [nopcommerce_prod].[dbo].[CustomerAddresses]
	END

	--- Customer > CustomerAttribute
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[CustomerAttribute]'

	--- Customer -> StoreMapping
	DELETE FROM [nopcommerce_prod].[dbo].[StoreMapping] WHERE [EntityName] = 'Customer'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[StoreMapping] NOCHECK CONSTRAINT ALL
	
		INSERT INTO [nopcommerce_prod].[dbo].[StoreMapping]
			   ([EntityId]
			   ,[EntityName]
			   ,[StoreId])
		SELECT
				Customer.Id, 
				'Customer',
				CustomerSourceSites.SourceSite_Id
			FROM
				smwbluedot_prod.dbo.Customer AS SMWCustomer 
				INNER JOIN smwbluedot_prod.dbo.CustomerSourceSites AS CustomerSourceSites
					ON SMWCustomer.Id = CustomerSourceSites.Customer_Id 
				INNER JOIN
					nopcommerce.dbo.Customer AS Customer ON Customer.Id = SMWCustomer.Id

		INSERT INTO [nopcommerce_prod].[dbo].[StoreMapping]
			   ([EntityId]
			   ,[EntityName]
			   ,[StoreId])
		SELECT
				Customer.Id, 
				'Customer',
				SMWCustomer.SourceSiteId
			FROM
				smwbluedot_prod.dbo.Customer AS SMWCustomer 
				INNER JOIN
					nopcommerce.dbo.Customer AS Customer ON Customer.Id = SMWCustomer.Id
			WHERE SMWCustomer.SourceSiteId IS NOT NULL

		ALTER TABLE [nopcommerce_prod].[dbo].[StoreMapping] CHECK CONSTRAINT ALL

		SELECT 'StoreMapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[StoreMapping] WHERE EntityName = 'Customer' 
	END

	--- Discount
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Discount]'
	
	--- Discount > Discount_AppliedToCategories
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Discount_AppliedToCategories]'

	--- Discount > DiscountRequirement
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[DiscountRequirement]'

	--- Discount > DiscountUsageHistory
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[DiscountUsageHistory]'

	--- Download
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Download]'
	
	--- ExternalAuthenticationRecord
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ExternalAuthenticationRecord]'

	--- GiftCard
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[GiftCard]'

	--- GiftCard > GiftCardUsageHistory
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[GiftCardUsageHistory]'

	--- Manufacturer
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Manufacturer]'
	BEGIN
		
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Manufacturer] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Manufacturer] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Manufacturer]
           ([Id]
           ,[Name]
           ,[Description]
           ,[ManufacturerTemplateId]
           ,[MetaKeywords]
           ,[MetaDescription]
           ,[MetaTitle]
           ,[PictureId]
           ,[PageSize]
           ,[AllowCustomersToSelectPageSize]
           ,[PageSizeOptions]
           ,[PriceRanges]
           ,[SubjectToAcl]
           ,[LimitedToStores]
           ,[Published]
           ,[Deleted]
           ,[DisplayOrder]
           ,[CreatedOnUtc]
           ,[UpdatedOnUtc]
           ,[IsMaster]
           ,[MasterId])
		SELECT 
			[Id]
			,[Name]
           ,[Description]
           ,[ManufacturerTemplateId]
           ,NULL
           ,NULL
           ,NULL
           ,[PictureId]
           ,[PageSize]
           ,[AllowCustomersToSelectPageSize]
           ,[PageSizeOptions]
           ,[PriceRanges]
           ,0
           ,0
           ,[Published]
           ,[Deleted]
           ,[DisplayOrder]
           ,[CreatedOnUtc]
           ,[UpdatedOnUtc]
           ,0
           ,0
		FROM smwbluedot_prod.dbo.[Manufacturer]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Manufacturer] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Manufacturer] OFF
			   
		SELECT 'Manufacturer', COUNT(*) FROM [nopcommerce_prod].[dbo].[Manufacturer]
	END
	
	--- Manufacturer > ManufacturerTemplate
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ManufacturerTemplate]'	
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ManufacturerTemplate] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[ManufacturerTemplate] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ManufacturerTemplate]
           ([Id]
           ,[Name]
           ,[ViewPath]
           ,[DisplayOrder])
		SELECT 
			[Id]
           ,[Name]
           ,[ViewPath]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[ManufacturerTemplate]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ManufacturerTemplate] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ManufacturerTemplate] OFF
			   
		SELECT 'ManufacturerTemplate', COUNT(*) FROM [nopcommerce_prod].[dbo].[ManufacturerTemplate]
	END

	--- MessageTemplate 
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[MessageTemplate]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[MessageTemplate] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[MessageTemplate] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[MessageTemplate]
           ([Id]
		   ,[Name]
           ,[BccEmailAddresses]
           ,[Subject]
           ,[Body]
           ,[IsActive]
           ,[AttachedDownloadId]
           ,[EmailAccountId]
           ,[LimitedToStores])
		SELECT
           [Id]
		   ,[Name]
           ,[BccEmailAddresses]
           ,[Subject]
           ,[Body]
           ,[IsActive]
           ,0
           ,1
           ,0
        FROM [smwbluedot_prod].[dbo].[MessageTemplate]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[MessageTemplate] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[MessageTemplate] OFF
		
		SELECT 'MessageTemplate', COUNT(*) FROM [nopcommerce_prod].[dbo].[MessageTemplate]
	END

	--- News 
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[News]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[News] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[News] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[News]
           ([Id]
		   ,[LanguageId]
           ,[Title]
           ,[Short]
           ,[Full]
           ,[Published]
           ,[StartDateUtc]
           ,[EndDateUtc]
           ,[AllowComments]
           ,[CommentCount]
           ,[LimitedToStores]
           ,[MetaKeywords]
           ,[MetaDescription]
           ,[MetaTitle]
           ,[CreatedOnUtc]
           ,[IsMaster]
           ,[MasterId])
		SELECT
           [Id]
		   ,[LanguageId]
           ,[Title]
           ,[Short]
           ,ISNULL([Full], '')
           ,[Published]
           ,CreatedOnUtc
           ,CreatedOnUtc
           ,[AllowComments]
           ,0
           ,1
           ,NULL
           ,NULL
           ,NULL
           ,[CreatedOnUtc]
           ,0
           ,0
        FROM [smwbluedot_prod].[dbo].[News]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[News] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[News] OFF
		
		SELECT 'News', COUNT(*) FROM [nopcommerce_prod].[dbo].[News]
	END
	
	--- News -> StoreMapping
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[StoreMapping] NOCHECK CONSTRAINT ALL
	
		INSERT INTO [nopcommerce_prod].[dbo].[StoreMapping]
			   ([EntityId]
			   ,[EntityName]
			   ,[StoreId])
		SELECT
				Id,
				'News',
				SourceSiteId
			FROM [smwbluedot_prod].[dbo].[News]

		ALTER TABLE [nopcommerce_prod].[dbo].[StoreMapping] CHECK CONSTRAINT ALL

		SELECT 'StoreMapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[StoreMapping] WHERE EntityName = 'News' 
	END

	--- News > NewsComment 
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[NewsComment]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[NewsComment] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[NewsComment] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[NewsComment]
           ([Id]
		   ,[CommentTitle]
           ,[CommentText]
           ,[NewsItemId]
           ,[CustomerId]
           ,[CreatedOnUtc])
		SELECT
           [Id]
		   ,[CommentTitle]
           ,[CommentText]
           ,[NewsItemId]
           ,0
           ,GETDATE()
        FROM [smwbluedot_prod].[dbo].[NewsComment]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[NewsComment] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[NewsComment] OFF
		
		SELECT 'NewsComment', COUNT(*) FROM [nopcommerce_prod].[dbo].[NewsComment]
	END

	--- News > NewsLetterSubscription 
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[NewsLetterSubscription]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[NewsLetterSubscription] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[NewsLetterSubscription] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[NewsLetterSubscription]
           ([Id]
		   ,[NewsLetterSubscriptionGuid]
           ,[Email]
           ,[Active]
           ,[StoreId]
           ,[CreatedOnUtc])
		SELECT
           [Id]
		   ,[NewsLetterSubscriptionGuid]
           ,[Email]
           ,[Active]
           ,0
           ,[CreatedOnUtc]
        FROM [smwbluedot_prod].[dbo].[NewsLetterSubscription]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[NewsLetterSubscription] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[NewsLetterSubscription] OFF
		
		SELECT 'NewsLetterSubscription', COUNT(*) FROM [nopcommerce_prod].[dbo].[NewsLetterSubscription]
	END
	
	--- Product_Manufacturer_Mapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping]
           ([Id]
           ,[ProductId]
           ,[ManufacturerId]
           ,[IsFeaturedProduct]
           ,[DisplayOrder])
		SELECT 
			[Id]
           ,[ProductId]
           ,[ManufacturerId]
           ,[IsFeaturedProduct]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[Product_Manufacturer_Mapping]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping] OFF
			   
		SELECT 'Product_Manufacturer_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_Manufacturer_Mapping]
	END
	
	--- Order 
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Order]'
	BEGIN
		
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Order] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Order] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Order]
           ([Id]
		   ,[OrderGuid]
           ,[StoreId]
           ,[CustomerId]
           ,[BillingAddressId]
           ,[ShippingAddressId]
           ,[PickUpInStore]
           ,[OrderStatusId]
           ,[ShippingStatusId]
           ,[PaymentStatusId]
           ,[PaymentMethodSystemName]
           ,[CustomerCurrencyCode]
           ,[CurrencyRate]
           ,[CustomerTaxDisplayTypeId]
           ,[VatNumber]
           ,[OrderSubtotalInclTax]
           ,[OrderSubtotalExclTax]
           ,[OrderSubTotalDiscountInclTax]
           ,[OrderSubTotalDiscountExclTax]
           ,[OrderShippingInclTax]
           ,[OrderShippingExclTax]
           ,[PaymentMethodAdditionalFeeInclTax]
           ,[PaymentMethodAdditionalFeeExclTax]
           ,[TaxRates]
           ,[OrderTax]
           ,[OrderDiscount]
           ,[OrderTotal]
           ,[RefundedAmount]
           ,[RewardPointsWereAdded]
           ,[CheckoutAttributeDescription]
           ,[CheckoutAttributesXml]
           ,[CustomerLanguageId]
           ,[AffiliateId]
           ,[CustomerIp]
           ,[AllowStoringCreditCardNumber]
           ,[CardType]
           ,[CardName]
           ,[CardNumber]
           ,[MaskedCreditCardNumber]
           ,[CardCvv2]
           ,[CardExpirationMonth]
           ,[CardExpirationYear]
           ,[AuthorizationTransactionId]
           ,[AuthorizationTransactionCode]
           ,[AuthorizationTransactionResult]
           ,[CaptureTransactionId]
           ,[CaptureTransactionResult]
           ,[SubscriptionTransactionId]
           ,[PaidDateUtc]
           ,[ShippingMethod]
           ,[ShippingRateComputationMethodSystemName]
           ,[CustomValuesXml]
           ,[Deleted]
           ,[CreatedOnUtc])
     SELECT
           [Id]
		   ,[OrderGuid]
           ,SourceSiteId
           ,[CustomerId]
           ,[BillingAddressId]
           ,[ShippingAddressId]
           ,0
           ,[OrderStatusId]
           ,[ShippingStatusId]
           ,[PaymentStatusId]
           ,[PaymentMethodSystemName]
           ,[CustomerCurrencyCode]
           ,[CurrencyRate]
           ,[CustomerTaxDisplayTypeId]
           ,[VatNumber]
           ,[OrderSubtotalInclTax]
           ,[OrderSubtotalExclTax]
           ,[OrderSubTotalDiscountInclTax]
           ,[OrderSubTotalDiscountExclTax]
           ,[OrderShippingInclTax]
           ,[OrderShippingExclTax]
           ,[PaymentMethodAdditionalFeeInclTax]
           ,[PaymentMethodAdditionalFeeExclTax]
           ,[TaxRates]
           ,[OrderTax]
           ,[OrderDiscount]
           ,[OrderTotal]
           ,[RefundedAmount]
           ,[RewardPointsWereAdded]
           ,[CheckoutAttributeDescription]
           ,[CheckoutAttributesXml]
           ,[CustomerLanguageId]
           ,0
           ,[CustomerIp]
           ,[AllowStoringCreditCardNumber]
           ,[CardType]
           ,[CardName]
           ,[CardNumber]
           ,[MaskedCreditCardNumber]
           ,[CardCvv2]
           ,[CardExpirationMonth]
           ,[CardExpirationYear]
           ,[AuthorizationTransactionId]
           ,[AuthorizationTransactionCode]
           ,[AuthorizationTransactionResult]
           ,[CaptureTransactionId]
           ,[CaptureTransactionResult]
           ,[SubscriptionTransactionId]
           ,[PaidDateUtc]
           ,[ShippingMethod]
           ,[ShippingRateComputationMethodSystemName]
           ,NULL
           ,[Deleted]
           ,[CreatedOnUtc]
        FROM [smwbluedot_prod].[dbo].[Order]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Order] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Order] OFF
		
		SELECT 'Order', COUNT(*) FROM [nopcommerce_prod].[dbo].[Order]
	END
	
	--- Order > OrderItem
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[OrderItem]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[OrderItem] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[OrderItem] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[OrderItem]
           ([Id]
		   ,[OrderItemGuid]
           ,[OrderId]
           ,[ProductId]
           ,[Quantity]
           ,[UnitPriceInclTax]
           ,[UnitPriceExclTax]
           ,[PriceInclTax]
           ,[PriceExclTax]
           ,[DiscountAmountInclTax]
           ,[DiscountAmountExclTax]
           ,[OriginalProductCost]
           ,[AttributeDescription]
           ,[AttributesXml]
           ,[DownloadCount]
           ,[IsDownloadActivated]
           ,[LicenseDownloadId]
           ,[ItemWeight]
           ,[RentalStartDateUtc]
           ,[RentalEndDateUtc])
     SELECT
           osp.[Id]
		   ,osp.OrderSiteProductGuid
           ,osp.[OrderId]
           ,p.[Id]
           ,osp.[Quantity]
           ,osp.[UnitPriceInclTax]
           ,osp.[UnitPriceExclTax]
           ,osp.[PriceInclTax]
           ,osp.[PriceExclTax]
           ,osp.[DiscountAmountInclTax]
           ,osp.[DiscountAmountExclTax]
           ,0
           ,osp.[AttributeDescription]
           ,osp.[AttributesXml]
           ,osp.[DownloadCount]
           ,osp.[IsDownloadActivated]
           ,osp.[LicenseDownloadId]
           ,0
           ,NULL
           ,NULL
        FROM [smwbluedot_prod].[dbo].[OrderSiteProduct] osp INNER JOIN [nopcommerce_prod].[dbo].[Product] p
			ON osp.SiteProductId = p.SiteProductId
		
		ALTER TABLE [nopcommerce_prod].[dbo].[OrderItem] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[OrderItem] OFF
		
		SELECT 'OrderItem', COUNT(*) FROM [nopcommerce_prod].[dbo].[OrderItem]
	END
	
	--- Order > OrderNote
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[OrderNote]'	
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[OrderNote] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[OrderNote] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[OrderNote]
           ([Id]
		   ,[OrderId]
           ,[Note]
           ,[DownloadId]
           ,[DisplayToCustomer]
           ,[CreatedOnUtc])
		SELECT
           [Id]
		   ,[OrderId]
           ,[Note]
           ,0
           ,[DisplayToCustomer]
           ,[CreatedOnUtc]
        FROM [smwbluedot_prod].[dbo].[OrderNote]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[OrderNote] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[OrderNote] OFF
		
		SELECT 'OrderNote', COUNT(*) FROM [nopcommerce_prod].[dbo].[OrderNote]
	END
	
	--- PermissionRecord
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]'
	BEGIN	
		
		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord]
			   ([Name]
			   ,[SystemName]
			   ,[Category])
		 VALUES
			   ('Admin area. Manage Customers. Manage Records'
			   ,'ManageCustomersManageRecords'
			   ,'Customers')
		
		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord]
			   ([Name]
			   ,[SystemName]
			   ,[Category])
		 VALUES
			   ('Admin area. Manage Customers. Customer Roles'
			   ,'ManageCustomersCustomerRoles'
			   ,'Customers')
			   
		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord]
			   ([Name]
			   ,[SystemName]
			   ,[Category])
		 VALUES
			   ('Admin area. Manage Message Queue. Manage Records'
			   ,'ManageMessageQueueManageRecords'
			   ,'Configuration')
			   
		SELECT 'PermissionRecord', COUNT(*) FROM [nopcommerce_prod].[dbo].[PermissionRecord]
	END
	
	--- PermissionRecord > PermissionRecord_Role_Mapping
	DELETE FROM [nopcommerce_prod].[dbo].[PermissionRecord]
		WHERE Name LIKE 'Admin area. Manage Customers.%'
	DELETE FROM [nopcommerce_prod].[dbo].[PermissionRecord]
		WHERE Name LIKE 'Admin area. Manage Message Queue.%'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping] NOCHECK CONSTRAINT ALL
			   
		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,1
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]
			   
		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,2
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]
			   WHERE SystemName = 'AccessAdminPanel'
			   OR SystemName = 'ManageForums'
			   OR Name LIKE 'Public%'
			   
		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,3
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]
			   WHERE Name LIKE 'Public%'

		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,4
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]
			   WHERE Name LIKE 'Public%'

		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,5
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]
			   WHERE SystemName = 'AccessAdminPanel'
			   OR SystemName = 'ManageProducts'
			   OR SystemName = 'ManageOrders'

		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,6
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]

		INSERT INTO [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
			   ([PermissionRecord_Id]
			   ,[CustomerRole_Id])
			SELECT 
			   Id
			   ,7
			   FROM [nopcommerce_prod].[dbo].[PermissionRecord]

		ALTER TABLE [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping] CHECK CONSTRAINT ALL

		SELECT 'PermissionRecord_Role_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[PermissionRecord_Role_Mapping]
	END
	
	--- Picture
	DECLARE @resetPictures BIT
	SET @resetPictures = 0
	IF @resetPictures = 1
		BEGIN
			EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Picture]'
		END
	BEGIN
		IF @resetPictures = 1
		BEGIN
			DELETE FROM [nopcommerce_prod].[dbo].[Picture]

			SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Picture] ON
			ALTER TABLE [nopcommerce_prod].[dbo].[Picture] NOCHECK CONSTRAINT ALL
			
			INSERT INTO [nopcommerce_prod].[dbo].[Picture]
				   ([Id]
				   ,[PictureBinary]
				   ,[MimeType]
				   ,[SeoFilename]
				   ,[AltAttribute]
				   ,[TitleAttribute]
				   ,[IsNew])
			 SELECT Id
				   ,PictureBinary
				   ,MimeType
				   ,[Filename]
				   ,[Filename]
				   ,[Filename]
				   ,IsNew
			FROM smwbluedot_prod.[dbo].[Picture]

			ALTER TABLE [nopcommerce_prod].[dbo].[Picture] CHECK CONSTRAINT ALL
			SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Picture] OFF

			SELECT 'Picture', COUNT(*) FROM [nopcommerce_prod].[dbo].[Picture]
		END
	END
	
	--- Poll 
	BEGIN
		EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[PollAnswer]'
		EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[PollVotingRecord]'
		EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Poll]'
	
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Poll] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Poll] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Poll]
           ([Id]
           ,[LanguageId]
           ,[Name]
           ,[SystemKeyword]
           ,[Published]
           ,[ShowOnHomePage]
           ,[AllowGuestsToVote]
           ,[DisplayOrder]
           ,[StartDateUtc]
           ,[EndDateUtc]
           ,[LimitedToStores]
           ,[IsMaster]
           ,[MasterId])
		SELECT
           [Id]
           ,[LanguageId]
           ,[Name]
           ,[SystemKeyword]
           ,[Published]
           ,[ShowOnHomePage]
           ,1
           ,[DisplayOrder]
           ,[StartDateUtc]
           ,[EndDateUtc]
           ,0
           ,0
           ,0
        FROM [smwbluedot_prod].[dbo].[Poll]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Poll] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Poll] OFF
		
		SELECT 'Poll', COUNT(*) FROM [nopcommerce_prod].[dbo].[Poll]
	END

	--- Poll > PollAnswer
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[PollAnswer] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[PollAnswer] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[PollAnswer]
           ([Id]
           ,[PollId]
           ,[Name]
           ,[NumberOfVotes]
           ,[DisplayOrder])
		SELECT
           [Id]
           ,[PollId]
           ,[Name]
           ,[NumberOfVotes]
           ,[DisplayOrder]
        FROM [smwbluedot_prod].[dbo].[PollAnswer]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[PollAnswer] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[PollAnswer] OFF
		
		SELECT 'PollAnswer', COUNT(*) FROM [nopcommerce_prod].[dbo].[PollAnswer]
	END

	--- Poll > PollVotingRecord
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[PollVotingRecord] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[PollVotingRecord] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[PollVotingRecord]
           ([Id]
           ,[PollAnswerId]
           ,[CustomerId]
           ,[CreatedOnUtc])
		SELECT
           [Id]
           ,[PollAnswerId]
           ,0
           ,null
        FROM [smwbluedot_prod].[dbo].[PollVotingRecord]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[PollVotingRecord] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[PollVotingRecord] OFF
		
		SELECT 'PollVotingRecord', COUNT(*) FROM [nopcommerce_prod].[dbo].[PollVotingRecord]
	END

	--- Master Product --- @TO DO: DROP SiteProductId
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Product] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product]
			   ([Id]
			   ,[ProductTypeId]
			   ,[ParentGroupedProductId]
			   ,[VisibleIndividually]
			   ,[Name]
			   ,[ShortDescription]
			   ,[FullDescription]
			   ,[AdminComment]
			   ,[ProductTemplateId]
			   ,[VendorId]
			   ,[ShowOnHomePage]
			   ,[MetaKeywords]
			   ,[MetaDescription]
			   ,[MetaTitle]
			   ,[AllowCustomerReviews]
			   ,[ApprovedRatingSum]
			   ,[NotApprovedRatingSum]
			   ,[ApprovedTotalReviews]
			   ,[NotApprovedTotalReviews]
			   ,[SubjectToAcl]
			   ,[LimitedToStores]
			   ,[Sku]
			   ,[ManufacturerPartNumber]
			   ,[Gtin]
			   ,[IsGiftCard]
			   ,[GiftCardTypeId]
			   ,[OverriddenGiftCardAmount]
			   ,[RequireOtherProducts]
			   ,[RequiredProductIds]
			   ,[AutomaticallyAddRequiredProducts]
			   ,[IsDownload]
			   ,[DownloadId]
			   ,[UnlimitedDownloads]
			   ,[MaxNumberOfDownloads]
			   ,[DownloadExpirationDays]
			   ,[DownloadActivationTypeId]
			   ,[HasSampleDownload]
			   ,[SampleDownloadId]
			   ,[HasUserAgreement]
			   ,[UserAgreementText]
			   ,[IsRecurring]
			   ,[RecurringCycleLength]
			   ,[RecurringCyclePeriodId]
			   ,[RecurringTotalCycles]
			   ,[IsRental]
			   ,[RentalPriceLength]
			   ,[RentalPricePeriodId]
			   ,[IsShipEnabled]
			   ,[IsFreeShipping]
			   ,[ShipSeparately]
			   ,[AdditionalShippingCharge]
			   ,[DeliveryDateId]
			   ,[IsTaxExempt]
			   ,[TaxCategoryId]
			   ,[IsTelecommunicationsOrBroadcastingOrElectronicServices]
			   ,[ManageInventoryMethodId]
			   ,[UseMultipleWarehouses]
			   ,[WarehouseId]
			   ,[StockQuantity]
			   ,[DisplayStockAvailability]
			   ,[DisplayStockQuantity]
			   ,[MinStockQuantity]
			   ,[LowStockActivityId]
			   ,[NotifyAdminForQuantityBelow]
			   ,[BackorderModeId]
			   ,[AllowBackInStockSubscriptions]
			   ,[OrderMinimumQuantity]
			   ,[OrderMaximumQuantity]
			   ,[AllowedQuantities]
			   ,[AllowAddingOnlyExistingAttributeCombinations]
			   ,[DisableBuyButton]
			   ,[DisableWishlistButton]
			   ,[AvailableForPreOrder]
			   ,[PreOrderAvailabilityStartDateTimeUtc]
			   ,[CallForPrice]
			   ,[Price]
			   ,[OldPrice]
			   ,[ProductCost]
			   ,[SpecialPrice]
			   ,[SpecialPriceStartDateTimeUtc]
			   ,[SpecialPriceEndDateTimeUtc]
			   ,[CustomerEntersPrice]
			   ,[MinimumCustomerEnteredPrice]
			   ,[MaximumCustomerEnteredPrice]
			   ,[BasepriceEnabled]
			   ,[BasepriceAmount]
			   ,[BasepriceUnitId]
			   ,[BasepriceBaseAmount]
			   ,[BasepriceBaseUnitId]
			   ,[MarkAsNew]
			   ,[MarkAsNewStartDateTimeUtc]
			   ,[MarkAsNewEndDateTimeUtc]
			   ,[HasTierPrices]
			   ,[HasDiscountsApplied]
			   ,[Weight]
			   ,[Length]
			   ,[Width]
			   ,[Height]
			   ,[AvailableStartDateTimeUtc]
			   ,[AvailableEndDateTimeUtc]
			   ,[DisplayOrder]
			   ,[Published]
			   ,[Deleted]
			   ,[CreatedOnUtc]
			   ,[UpdatedOnUtc]
			   ,[IsMaster]
			   ,[MasterId]
			   ,[IsMealPlan]
			   ,[IsDonation])
		SELECT
				p.[Id]
			   ,5
			   ,0
			   ,1
			   ,p.[Name]
			   ,p.[ShortDescription]
			   ,p.[FullDescription]
			   ,p.[AdminComment]
			   ,1
			   ,0
			   ,0
			   ,p.[MetaKeywords]
			   ,p.[MetaDescription]
			   ,p.[MetaTitle]
			   ,p.[AllowCustomerReviews]
			   ,p.[ApprovedRatingSum]
			   ,p.[NotApprovedRatingSum]
			   ,p.[ApprovedTotalReviews]
			   ,p.[NotApprovedTotalReviews]
			   ,0
			   ,0
			   ,pv.[Sku]
			   ,pv.[ManufacturerPartNumber]
			   ,pv.[Gtin]
			   ,pv.[IsGiftCard]
			   ,pv.[GiftCardTypeId]
			   ,0
			   ,pv.[RequireOtherProducts]
			   ,NULL
			   ,0
			   ,pv.[IsDownload]
			   ,pv.[DownloadId]
			   ,pv.[UnlimitedDownloads]
			   ,pv.[MaxNumberOfDownloads]
			   ,pv.[DownloadExpirationDays]
			   ,pv.[DownloadActivationTypeId]
			   ,pv.[HasSampleDownload]
			   ,pv.[SampleDownloadId]
			   ,pv.[HasUserAgreement]
			   ,pv.[UserAgreementText]
			   ,pv.[IsRecurring]
			   ,pv.[RecurringCycleLength]
			   ,pv.[RecurringCyclePeriodId]
			   ,pv.[RecurringTotalCycles]
			   ,0
			   ,0
			   ,0
			   ,pv.[IsShipEnabled]
			   ,pv.[IsFreeShipping]
			   ,0
			   ,pv.[AdditionalShippingCharge]
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,pv.[CallForPrice]
			   ,pv.[Price]
			   ,pv.[OldPrice]
			   ,pv.[ProductCost]
			   ,0
			   ,0
			   ,0
			   ,pv.[CustomerEntersPrice]
			   ,pv.[MinimumCustomerEnteredPrice]
			   ,pv.[MaximumCustomerEnteredPrice]
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,pv.[Weight]
			   ,pv.[Length]
			   ,pv.[Width]
			   ,pv.[Height]
			   ,0
			   ,0
			   ,0
			   ,0
			   ,p.[Deleted]
			   ,p.[CreatedOnUtc]
			   ,p.[UpdatedOnUtc]
			   ,1
			   ,0
			   ,pv.[IsMealPlan]
			   ,pv.[IsDonation]
			FROM smwbluedot_prod.dbo.Product p INNER JOIN smwbluedot_prod.dbo.ProductVariant pv 
				ON p.Id = pv.ProductId

		ALTER TABLE [nopcommerce_prod].[dbo].[Product] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product] OFF
		
		SELECT 'Product', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product]
	END

	--- Master Product > Product_Category_Mapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product_Category_Mapping]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_Category_Mapping] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Category_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_Category_Mapping]
           ([Id]
           ,[ProductId]
           ,[CategoryId]
           ,[IsFeaturedProduct]
           ,[DisplayOrder])
		SELECT [Id]
			,[ProductId]
			,[CategoryId]
			,[IsFeaturedProduct]
			,[DisplayOrder]
			FROM [smwbluedot_prod].[dbo].[Product_Category_Mapping]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Category_Mapping] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_Category_Mapping] OFF
			   
		SELECT 'Product_Category_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_Category_Mapping]
	END
	
	--- Master Product > Product_Picture_Mapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product_Picture_Mapping]'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Picture_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_Picture_Mapping]
           ([ProductId]
           ,[PictureId]
           ,[DisplayOrder])
		SELECT 
			[ProductId]
           ,[PictureId]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[Product_Picture_Mapping]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Picture_Mapping] CHECK CONSTRAINT ALL
			   
		SELECT 'Product_Picture_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_Picture_Mapping]
	END

	--- Master Product > Product_SpecificationAttribute_Mapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping]
           ([Id]
           ,[ProductId]
           ,[AttributeTypeId]
           ,[SpecificationAttributeOptionId]
           ,[CustomValue]
           ,[AllowFiltering]
           ,[ShowOnProductPage]
           ,[DisplayOrder])
		SELECT 
			[Id]
           ,[ProductId]
           ,NULL
           ,[SpecificationAttributeOptionId]
           ,NULL
           ,[AllowFiltering]
           ,[ShowOnProductPage]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[Product_SpecificationAttribute_Mapping]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping] OFF
			   
		SELECT 'Product_SpecificationAttribute_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_SpecificationAttribute_Mapping]
	END
	
	--- Master Product > Product_ProductAttribute_Mapping
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping]
           ([Id]
           ,[ProductId]
           ,[ProductAttributeId]
           ,[TextPrompt]
           ,[IsRequired]
           ,[AttributeControlTypeId]
           ,[DisplayOrder]
           ,[ValidationMinLength]
           ,[ValidationMaxLength]
           ,[ValidationFileAllowedExtensions]
           ,[ValidationFileMaximumSize]
           ,[DefaultValue]
           ,[ConditionAttributeXml])
		SELECT 
			ppm.[Id]
           ,pv.[ProductId]
           ,ppm.[ProductAttributeId]
           ,ppm.[TextPrompt]
           ,ppm.[IsRequired]
           ,ppm.[AttributeControlTypeId]
           ,ppm.[DisplayOrder]
           ,ppm.[ValidationMinLength]
           ,ppm.[ValidationMaxLength]
           ,NULL
           ,NULL
           ,ppm.[DefaultValue]
           ,NULL
		FROM smwbluedot_prod.dbo.ProductVariant_ProductAttribute_Mapping ppm INNER JOIN [smwbluedot_prod].[dbo].[ProductVariant] pv
			ON ppm.ProductVariantId = pv.Id
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] OFF
			   
		SELECT 'Product_ProductAttribute_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping]
	END

	--- ProductAttribute
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ProductAttribute]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductAttribute] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttribute] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductAttribute]
           ([Id]
           ,[Name]
           ,[Description])
		SELECT 
			[Id]
           ,[Name]
           ,[Description]
		FROM smwbluedot_prod.dbo.[ProductAttribute]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttribute] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductAttribute] OFF
			   
		SELECT 'ProductAttribute', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductAttribute]
	END
	
	--- ProductAttribute > ProductAttributeValue
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ProductAttributeValue]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductAttributeValue] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeValue] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductAttributeValue]
           ([Id]
           ,[ProductAttributeMappingId]
           ,[AttributeValueTypeId]
           ,[AssociatedProductId]
           ,[Name]
           ,[ColorSquaresRgb]
           ,[PriceAdjustment]
           ,[WeightAdjustment]
           ,[Cost]
           ,[Quantity]
           ,[IsPreSelected]
           ,[DisplayOrder]
           ,[PictureId])
		SELECT 
			pvav.[Id]
           ,pvav.[ProductVariantAttributeId]
           ,0
           ,0
           ,pvav.[Name]
           ,NULL
           ,pvav.[PriceAdjustment]
           ,pvav.[WeightAdjustment]
           ,0
           ,0
           ,pvav.[IsPreSelected]
           ,pvav.[DisplayOrder]
           ,0
		FROM smwbluedot_prod.dbo.ProductVariantAttributeValue pvav
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeValue] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductAttributeValue] OFF
			   
		SELECT 'ProductAttributeValue', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductAttributeValue]
	END
	
	--- ProductAttribute > ProductAttributeCombination
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ProductAttributeCombination]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductAttributeCombination] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeCombination] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductAttributeCombination]
           ([Id]
           ,[ProductId]
           ,[AttributesXml]
           ,[StockQuantity]
           ,[AllowOutOfStockOrders]
           ,[Sku]
           ,[ManufacturerPartNumber]
           ,[Gtin]
           ,[OverriddenPrice]
           ,[NotifyAdminForQuantityBelow])
		SELECT 
			pvac.[Id]
           ,pv.[ProductId]
           ,pvac.[AttributesXml]
           ,pvac.[StockQuantity]
           ,pvac.[AllowOutOfStockOrders]
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
		FROM [smwbluedot_prod].[dbo].[ProductVariantAttributeCombination] pvac INNER JOIN [smwbluedot_prod].[dbo].[ProductVariant] pv
			ON pvac.ProductVariantId = pv.Id
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeCombination] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductAttributeCombination] OFF
			   
		SELECT 'ProductAttributeCombination', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductAttributeCombination]
	END

	--- ProductReview
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ProductReview]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductReview] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductReview] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductReview]
           ([Id]
           ,[CustomerId]
           ,[ProductId]
           ,[IsApproved]
           ,[Title]
           ,[ReviewText]
           ,[Rating]
           ,[HelpfulYesTotal]
           ,[HelpfulNoTotal]
           ,[CreatedOnUtc])
		SELECT 
			[Id]
           ,NULL
           ,[ProductId]
           ,NULL
           ,[Title]
           ,[ReviewText]
           ,[Rating]
           ,[HelpfulYesTotal]
           ,[HelpfulNoTotal]
           ,NULL
		FROM smwbluedot_prod.dbo.[ProductReview]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductReview] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductReview] OFF
			   
		SELECT 'ProductReview', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductReview]
	END

	--- ProductReview > ProductReviewHelpfulness
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ProductReviewHelpfulness]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductReviewHelpfulness] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductReviewHelpfulness] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductReviewHelpfulness]
           ([Id]
           ,[ProductReviewId]
           ,[WasHelpful]
           ,[CustomerId])
		SELECT 
			[Id]
           ,[ProductReviewId]
           ,[WasHelpful]
           ,NULL
		FROM smwbluedot_prod.dbo.[ProductReviewHelpfulness]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductReviewHelpfulness] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[ProductReviewHelpfulness] OFF
			   
		SELECT 'ProductReviewHelpfulness', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductReviewHelpfulness]
	END

	--- QueuedEmail
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[QueuedEmail]'
	
	--- RecurringPayment
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[RecurringPayment]'

	--- RecurringPayment > RecurringPaymentHistory
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[RecurringPaymentHistory]'
	
	--- RelatedProduct - @TO DO: Which product id is being used?
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[RecurringPaymentHistory]'

	--- ReturnRequest
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ReturnRequest]'

	--- RewardPointsHistory
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[RewardPointsHistory]'

	--- ShoppingCartItem
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ShoppingCartItem]'

	--- SpecificationAttribute
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[SpecificationAttribute]'	
	BEGIN

		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[SpecificationAttribute] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[SpecificationAttribute] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[SpecificationAttribute]
           ([Id]
           ,[Name]
           ,[DisplayOrder])
		SELECT 
			[Id]
           ,[Name]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[SpecificationAttribute]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[SpecificationAttribute] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[SpecificationAttribute] OFF
			   
		SELECT 'SpecificationAttribute', COUNT(*) FROM [nopcommerce_prod].[dbo].[SpecificationAttribute]
	END

	--- SpecificationAttribute > SpecificationAttributeOption
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[SpecificationAttributeOption]'
	BEGIN
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[SpecificationAttributeOption] ON
		ALTER TABLE [nopcommerce_prod].[dbo].[SpecificationAttributeOption] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[SpecificationAttributeOption]
           ([Id]
           ,[SpecificationAttributeId]
           ,[Name]
           ,[DisplayOrder])
		SELECT 
			[Id]
           ,[SpecificationAttributeId]
           ,[Name]
           ,[DisplayOrder]
		FROM smwbluedot_prod.dbo.[SpecificationAttributeOption]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[SpecificationAttributeOption] CHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[SpecificationAttributeOption] OFF
			   
		SELECT 'SpecificationAttributeOption', COUNT(*) FROM [nopcommerce_prod].[dbo].[SpecificationAttributeOption]
	END

	--- TaxCategory
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[TaxCategory]'
	BEGIN
	
		ALTER TABLE [nopcommerce_prod].[dbo].[TaxCategory] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[TaxCategory]
           ([Name]
           ,[DisplayOrder])
		SELECT DISTINCT
           [Name]
           ,[DisplayOrder]
        FROM [smwbluedot_prod].[dbo].[TaxCategory]
		
		ALTER TABLE [nopcommerce_prod].[dbo].[TaxCategory] CHECK CONSTRAINT ALL
		
		SELECT 'TaxCategory', COUNT(*) FROM [nopcommerce_prod].[dbo].[TaxCategory]
	END

	--- TaxCategory > TaxRate
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[TaxRate]'
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[TaxRate] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[TaxRate]
           ([StoreId]
           ,[TaxCategoryId]
           ,[CountryId]
           ,[StateProvinceId]
           ,[Zip]
           ,[Percentage])
		SELECT 
           SourceSiteId
           ,tc.Id
           ,0
           ,0
           ,0
           ,_tc.Rate
        FROM [smwbluedot_prod].[dbo].[TaxCategory] _tc INNER JOIN [nopcommerce_prod].dbo.TaxCategory tc
			ON tc.Name = _tc.Name
		
		ALTER TABLE [nopcommerce_prod].[dbo].[TaxRate] CHECK CONSTRAINT ALL
		
		SELECT 'TaxRate', COUNT(*) FROM [nopcommerce_prod].[dbo].[TaxRate]
	END

	--- TierPrice
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[TierPrice]'

	--- Topic
	EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Topic]'
	BEGIN
	
		ALTER TABLE [nopcommerce_prod].[dbo].[Topic] NOCHECK CONSTRAINT ALL
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Topic] ON
		
		INSERT INTO [nopcommerce_prod].[dbo].[Topic]
           ([Id]
		   ,[SystemName]
           ,[IncludeInSitemap]
           ,[IncludeInTopMenu]
           ,[IncludeInFooterColumn1]
           ,[IncludeInFooterColumn2]
           ,[IncludeInFooterColumn3]
           ,[DisplayOrder]
           ,[AccessibleWhenStoreClosed]
           ,[IsPasswordProtected]
           ,[Password]
           ,[Title]
           ,[Body]
           ,[TopicTemplateId]
           ,[MetaKeywords]
           ,[MetaDescription]
           ,[MetaTitle]
           ,[SubjectToAcl]
           ,[LimitedToStores]
           ,[IsMaster]
           ,[MasterId])
		SELECT 
           [Id]
		   ,[SystemName]
           ,[IncludeInSitemap]
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,[IsPasswordProtected]
           ,[Password]
           ,[Title]
           ,[Body]
           ,0
           ,NULL
           ,NULL
           ,NULL
           ,0
           ,0
           ,0
           ,0
        FROM [smwbluedot_prod].[dbo].[Topic]
		
		SET IDENTITY_INSERT [nopcommerce_prod].[dbo].[Topic] OFF
		ALTER TABLE [nopcommerce_prod].[dbo].[Topic] CHECK CONSTRAINT ALL
		
		SELECT 'Topic', COUNT(*) FROM [nopcommerce_prod].[dbo].[Topic]
	END

	--- EncriptionKey
	BEGIN
		UPDATE [Setting]
			SET [Setting].[Value] = [GlobalSetting].[Value]
			FROM [nopcommerce_prod].[dbo].[Setting] [Setting] INNER JOIN [smwbluedot_prod].[dbo].[GlobalSetting] [GlobalSetting]
				ON [Setting].Name = GlobalSetting.Name
			WHERE
				GlobalSetting.Name = 'securitysettings.encryptionkey'
	END

	--- ProductTag
	BEGIN
		EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[Product_ProductTag_Mapping]'
		EXECUTE [nopcommerce_prod].[dbo].[TruncateTable] '[nopcommerce_prod].[dbo].[ProductTag]'
		
		SELECT 'ProductTag', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductTag]
		SELECT 'Product_ProductTag_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_ProductTag_Mapping]
	END

	--- Local Product
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Product] NOCHECK CONSTRAINT ALL

		INSERT INTO [nopcommerce_prod].[dbo].[Product]
           ([SiteProductId]
           ,[ProductTypeId]
           ,[ParentGroupedProductId]
           ,[VisibleIndividually]
           ,[Name]
           ,[ShortDescription]
           ,[FullDescription]
           ,[AdminComment]
           ,[ProductTemplateId]
           ,[VendorId]
           ,[ShowOnHomePage]
           ,[MetaKeywords]
           ,[MetaDescription]
           ,[MetaTitle]
           ,[AllowCustomerReviews]
           ,[ApprovedRatingSum]
           ,[NotApprovedRatingSum]
           ,[ApprovedTotalReviews]
           ,[NotApprovedTotalReviews]
           ,[SubjectToAcl]
           ,[LimitedToStores]
           ,[Sku]
           ,[ManufacturerPartNumber]
           ,[Gtin]
           ,[IsGiftCard]
           ,[GiftCardTypeId]
           ,[OverriddenGiftCardAmount]
           ,[RequireOtherProducts]
           ,[RequiredProductIds]
           ,[AutomaticallyAddRequiredProducts]
           ,[IsDownload]
           ,[DownloadId]
           ,[UnlimitedDownloads]
           ,[MaxNumberOfDownloads]
           ,[DownloadExpirationDays]
           ,[DownloadActivationTypeId]
           ,[HasSampleDownload]
           ,[SampleDownloadId]
           ,[HasUserAgreement]
           ,[UserAgreementText]
           ,[IsRecurring]
           ,[RecurringCycleLength]
           ,[RecurringCyclePeriodId]
           ,[RecurringTotalCycles]
           ,[IsRental]
           ,[RentalPriceLength]
           ,[RentalPricePeriodId]
           ,[IsShipEnabled]
           ,[IsFreeShipping]
           ,[ShipSeparately]
           ,[AdditionalShippingCharge]
           ,[DeliveryDateId]
           ,[IsTaxExempt]
           ,[TaxCategoryId]
           ,[IsTelecommunicationsOrBroadcastingOrElectronicServices]
           ,[ManageInventoryMethodId]
           ,[UseMultipleWarehouses]
           ,[WarehouseId]
           ,[StockQuantity]
           ,[DisplayStockAvailability]
           ,[DisplayStockQuantity]
           ,[MinStockQuantity]
           ,[LowStockActivityId]
           ,[NotifyAdminForQuantityBelow]
           ,[BackorderModeId]
           ,[AllowBackInStockSubscriptions]
           ,[OrderMinimumQuantity]
           ,[OrderMaximumQuantity]
           ,[AllowedQuantities]
           ,[AllowAddingOnlyExistingAttributeCombinations]
           ,[DisableBuyButton]
           ,[DisableWishlistButton]
           ,[AvailableForPreOrder]
           ,[PreOrderAvailabilityStartDateTimeUtc]
           ,[CallForPrice]
           ,[Price]
           ,[OldPrice]
           ,[ProductCost]
           ,[SpecialPrice]
           ,[SpecialPriceStartDateTimeUtc]
           ,[SpecialPriceEndDateTimeUtc]
           ,[CustomerEntersPrice]
           ,[MinimumCustomerEnteredPrice]
           ,[MaximumCustomerEnteredPrice]
           ,[BasepriceEnabled]
           ,[BasepriceAmount]
           ,[BasepriceUnitId]
           ,[BasepriceBaseAmount]
           ,[BasepriceBaseUnitId]
           ,[MarkAsNew]
           ,[MarkAsNewStartDateTimeUtc]
           ,[MarkAsNewEndDateTimeUtc]
           ,[HasTierPrices]
           ,[HasDiscountsApplied]
           ,[Weight]
           ,[Length]
           ,[Width]
           ,[Height]
           ,[AvailableStartDateTimeUtc]
           ,[AvailableEndDateTimeUtc]
           ,[DisplayOrder]
           ,[Published]
           ,[Deleted]
           ,[CreatedOnUtc]
           ,[UpdatedOnUtc]
           ,[IsMaster]
           ,[MasterId]
           ,[IsMealPlan]
           ,[IsDonation])
     SELECT
			sp.[id]
           ,5
           ,0
           ,1
           ,sp.[Name]
           ,sp.[ShortDescription]
           ,sp.[Description]
           ,[AdminComment]
           ,0
           ,0
           ,sp.[ShowOnHomePage]
           ,NULL
           ,NULL
           ,NULL
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,1
           ,pv.[Sku]
           ,pv.[ManufacturerPartNumber]
           ,pv.[Gtin]
           ,pv.[IsGiftCard]
           ,pv.[GiftCardTypeId]
           ,0
           ,pv.[RequireOtherProducts]
           ,0
           ,0
           ,pv.[IsDownload]
           ,pv.[DownloadId]
           ,pv.[UnlimitedDownloads]
           ,pv.[MaxNumberOfDownloads]
           ,pv.[DownloadExpirationDays]
           ,pv.[DownloadActivationTypeId]
           ,pv.[HasSampleDownload]
           ,pv.[SampleDownloadId]
           ,pv.[HasUserAgreement]
           ,pv.[UserAgreementText]
           ,pv.[IsRecurring]
           ,pv.[RecurringCycleLength]
           ,pv.[RecurringCyclePeriodId]
           ,pv.[RecurringTotalCycles]
           ,0
           ,0
           ,0
           ,pv.[IsShipEnabled]
           ,pv.[IsFreeShipping]
           ,0
           ,pv.[AdditionalShippingCharge]
           ,0
           ,sp.[IsTaxExempt]
           ,sp.[TaxCategoryId]
           ,0
           ,sp.[ManageInventoryMethodId]
           ,0
           ,0
           ,sp.[StockQuantity]
           ,sp.[DisplayStockAvailability]
           ,sp.[DisplayStockQuantity]
           ,sp.[MinStockQuantity]
           ,sp.[LowStockActivityId]
           ,sp.[NotifyAdminForQuantityBelow]
           ,sp.[BackorderModeId]
           ,sp.[AllowBackInStockSubscriptions]
           ,sp.[OrderMinimumQuantity]
           ,sp.[OrderMaximumQuantity]
           ,0
           ,0
           ,sp.[DisableBuyButton]
           ,sp.[DisableWishlistButton]
           ,0
           ,0
           ,pv.[CallForPrice]
           ,sp.[Price]
           ,sp.[OldPrice]
           ,pv.[ProductCost]
           ,sp.[SpecialPrice]
           ,sp.[SpecialPriceStartDateTimeUtc]
           ,sp.[SpecialPriceEndDateTimeUtc]
           ,pv.[CustomerEntersPrice]
           ,pv.[MinimumCustomerEnteredPrice]
           ,pv.[MaximumCustomerEnteredPrice]
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,pv.[Weight]
           ,pv.[Length]
           ,pv.[Width]
           ,pv.[Height]
           ,sp.[AvailableStartDateTimeUtc]
           ,sp.[AvailableEndDateTimeUtc]
           ,sp.[DisplayOrder]
           ,sp.[Published]
           ,sp.[Deleted]
           ,pv.[CreatedOnUtc]
           ,pv.[UpdatedOnUtc]
           ,0
           ,pv.ProductId
           ,pv.[IsMealPlan]
		   ,pv.[IsDonation]
        FROM 
			[smwbluedot_prod].[dbo].[SiteProduct] sp INNER JOIN [smwbluedot_prod].[dbo].[ProductVariant] pv
				ON sp.ProductVariantId = pv.Id

		ALTER TABLE [nopcommerce_prod].[dbo].[Product] CHECK CONSTRAINT ALL
		
		SELECT 'Product', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product]
			WHERE [SiteProductId] IS NOT NULL
	END
	
	--- Local Product > UrlRecord
	DELETE FROM [nopcommerce_prod].[dbo].[UrlRecord] WHERE EntityName = 'Product'
	BEGIN
		INSERT INTO [nopcommerce_prod].[dbo].[UrlRecord]
			   ([EntityId]
			   ,[EntityName]
			   ,[Slug]
			   ,[IsActive]
			   ,[LanguageId])
		 SELECT 
			   [Id]
			   ,'Product'
			   ,[nopcommerce_prod].[dbo].createSlug('Product', [nopcommerce_prod].[dbo].[Product].Name, 0)
			   ,[Published]
			   ,0
			FROM [nopcommerce_prod].[dbo].[Product]
				WHERE [SiteProductId] IS NOT NULL
			   
		SELECT 'UrlRecord', COUNT(*) FROM [nopcommerce_prod].[dbo].[UrlRecord]
			WHERE EntityName = 'Product'
	END
	
	--- Local Product > Master Product_Picture_Mapping
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Picture_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_Picture_Mapping]
           ([ProductId]
           ,[PictureId]
           ,[DisplayOrder])
		SELECT 
			p.[Id]
           ,[PictureId]
           ,spm.[DisplayOrder]
		FROM smwbluedot_prod.dbo.[SiteProduct_Picture_Mapping] spm INNER JOIN smwbluedot_prod.dbo.SiteProduct sp
			ON spm.SiteProductId = sp.Id
			INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON sp.Id = p.SiteProductId
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Picture_Mapping] CHECK CONSTRAINT ALL
			   
		SELECT 'Product_Picture_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_Picture_Mapping]
	END

	--- Local Product > Product_Category_Mapping
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Category_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_Category_Mapping]
           ([ProductId]
           ,[CategoryId]
           ,[IsFeaturedProduct]
           ,[DisplayOrder])
		SELECT p.[Id]
			,ssm.[SiteCategoryId]
			,ssm.[IsFeaturedProduct]
			,ssm.[DisplayOrder]
			FROM [smwbluedot_prod].[dbo].[SiteProduct_SiteCategory_Mapping] ssm INNER JOIN [smwbluedot_prod].[dbo].[SiteProduct] sp
				ON ssm.SiteProductId = sp.Id
				INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON sp.Id = p.SiteProductId
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Category_Mapping] CHECK CONSTRAINT ALL
			   
		SELECT 'Product_Category_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_Category_Mapping]
	END

	--- Local Product > Product_ProductAttribute_Mapping --- @TO DO: DROP SiteProductVariantAttributeId
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping]
           ([ProductId]
           ,[ProductAttributeId]
           ,[TextPrompt]
           ,[IsRequired]
           ,[AttributeControlTypeId]
           ,[DisplayOrder]
           ,[ValidationMinLength]
           ,[ValidationMaxLength]
           ,[ValidationFileAllowedExtensions]
           ,[ValidationFileMaximumSize]
           ,[DefaultValue]
           ,[ConditionAttributeXml]
           ,[SiteProductVariantAttributeId])
		SELECT 
           p.[Id]
           ,sppm.[ProductAttributeId]
           ,sppm.[TextPrompt]
           ,sppm.[IsRequired]
           ,sppm.[AttributeControlTypeId]
           ,sppm.[DisplayOrder]
           ,sppm.[ValidationMinLength]
           ,sppm.[ValidationMaxLength]
           ,NULL
           ,NULL
           ,sppm.[DefaultValue]
           ,NULL
           ,sppm.[Id]
		FROM smwbluedot_prod.dbo.SiteProduct_ProductAttribute_Mapping sppm INNER JOIN [smwbluedot_prod].[dbo].SiteProduct sp
			ON sppm.SiteProductId = sp.Id
			INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON sp.Id = p.SiteProductId
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] CHECK CONSTRAINT ALL
			   
		SELECT 'Product_ProductAttribute_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping]
	END
	
	--- Local Product > ProductAttributeValue
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeValue] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductAttributeValue]
           ([ProductAttributeMappingId]
           ,[AttributeValueTypeId]
           ,[AssociatedProductId]
           ,[Name]
           ,[ColorSquaresRgb]
           ,[PriceAdjustment]
           ,[WeightAdjustment]
           ,[Cost]
           ,[Quantity]
           ,[IsPreSelected]
           ,[DisplayOrder]
           ,[PictureId])
		SELECT 
			pppm.Id
           ,0
           ,0
           ,spav.[Name]
           ,NULL
           ,spav.[PriceAdjustment]
           ,spav.[WeightAdjustment]
           ,0
           ,0
           ,spav.[IsPreSelected]
           ,spav.[DisplayOrder]
           ,0
		FROM [smwbluedot_prod].[dbo].[SiteProductAttributeValue] spav INNER JOIN [nopcommerce_prod].[dbo].[Product_ProductAttribute_Mapping] pppm
			ON spav.SiteProductVariantAttributeId = pppm.SiteProductVariantAttributeId
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeValue] CHECK CONSTRAINT ALL
			   
		SELECT 'ProductAttributeValue', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductAttributeValue]
	END

	--- Local Product > Product_Category_Mapping
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Category_Mapping] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[Product_Category_Mapping]
           ([ProductId]
           ,[CategoryId]
           ,[IsFeaturedProduct]
           ,[DisplayOrder])
		SELECT 
			p.[Id]
			,pcm.CategoryId
			,pcm.IsFeaturedProduct
			,pcm.DisplayOrder
			FROM [nopcommerce_prod].[dbo].[Product] p INNER JOIN [nopcommerce_prod].[dbo].[Product_Category_Mapping] pcm
				ON p.[MasterId] = pcm.[ProductId]
			WHERE p.[MasterId] > 0
		
		ALTER TABLE [nopcommerce_prod].[dbo].[Product_Category_Mapping] CHECK CONSTRAINT ALL
			   
		SELECT 'Product_Category_Mapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[Product_Category_Mapping]
	END
	
	--- Local Product > ProductAttributeCombination
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeCombination] NOCHECK CONSTRAINT ALL
		
		INSERT INTO [nopcommerce_prod].[dbo].[ProductAttributeCombination]
           ([ProductId]
           ,[AttributesXml]
           ,[StockQuantity]
           ,[AllowOutOfStockOrders]
           ,[Sku]
           ,[ManufacturerPartNumber]
           ,[Gtin]
           ,[OverriddenPrice]
           ,[NotifyAdminForQuantityBelow])
		SELECT 
			p.[Id]
           ,spac.[AttributesXml]
           ,spac.[StockQuantity]
           ,spac.[AllowOutOfStockOrders]
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
		FROM [smwbluedot_prod].[dbo].SiteProductAttributeCombination spac INNER JOIN [smwbluedot_prod].[dbo].SiteProduct sp
			ON spac.SiteProductId = sp.Id
			INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON sp.Id = p.SiteProductId
		
		ALTER TABLE [nopcommerce_prod].[dbo].[ProductAttributeCombination] CHECK CONSTRAINT ALL
			   
		SELECT 'ProductAttributeCombination', COUNT(*) FROM [nopcommerce_prod].[dbo].[ProductAttributeCombination]
	END

	--- Local Product -> StoreMapping
	BEGIN
		ALTER TABLE [nopcommerce_prod].[dbo].[StoreMapping] NOCHECK CONSTRAINT ALL
	
		INSERT INTO [nopcommerce_prod].[dbo].[StoreMapping]
			   ([EntityId]
			   ,[EntityName]
			   ,[StoreId])
		SELECT
				p.[Id], 
				'Product',
				sp.SourceSiteId
			FROM
				smwbluedot_prod.dbo.SiteProduct sp INNER JOIN [nopcommerce_prod].[dbo].[Product] p
				ON sp.Id = p.SiteProductId

		ALTER TABLE [nopcommerce_prod].[dbo].[StoreMapping] CHECK CONSTRAINT ALL

		SELECT 'StoreMapping', COUNT(*) FROM [nopcommerce_prod].[dbo].[StoreMapping] WHERE EntityName = 'Product' 
	END
END