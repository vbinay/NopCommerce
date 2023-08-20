/*************************************************************************************************************
Product Table Changes
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the IsBundleProduct in the Product table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
	
--------------------------------------
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'IsBundleProduct')
	)
	BEGIN
			ALTER TABLE [Product] ADD [IsBundleProduct] [bit] NULL 
			CONSTRAINT DF_Product_IsBundleProduct  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsBundleProduct - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'IsBundleProduct')
	)
 BEGIN
			UPDATE [Product] SET 	[IsBundleProduct] = 0 
	END

GO

----------------------------------------------------------------------
/*************************************************************************************************************
ShoppingCartItem Table Changes

Script			: Alter table ShoppingCartItem - add IsBundleProduct column With Default Values
Purpose			: To Add the IsBundleProduct in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
	
--------------------------------------
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'IsBundleProduct')
	)
	BEGIN
			ALTER TABLE [ShoppingCartItem] ADD [IsBundleProduct] [bit] NULL 
			CONSTRAINT DF_ShoppingCartItem_IsBundleProduct  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsBundleProduct - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'IsBundleProduct')
	)
 BEGIN
			UPDATE [ShoppingCartItem] SET 	[IsBundleProduct] = 0 
	END

GO

---------------------------------------------------------------

/*************************************************************************************************************
OrderItem Table Changes

Script			: Alter table OrderItem  - add column With Default Values
Purpose			: To Add the IsBundleProduct in the Product table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		AND (sys.columns.[name] LIKE 'IsBundleProduct')
	)
	BEGIN
			ALTER TABLE [OrderItem] ADD [IsBundleProduct] [bit] NULL 
			CONSTRAINT DF_OrderItem_IsBundleProduct  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsBundleProduct - Column already exists in OrderItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		AND (sys.columns.[name] LIKE 'IsBundleProduct')
	)
 BEGIN
			UPDATE [OrderItem] SET 	[IsBundleProduct] = 0 
	END

GO
/*************************************************************************************************************
Create Table - ShoppingCartBundleProductItem

Script			: Create Table ShoppingCartBundleProductItem
Purpose			: Create Table ShoppingCartBundleProductItem
Developed By	: Cognizant Technology Solutions
Date			: March 2021
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
	)
	BEGIN
 CREATE TABLE [dbo].[ShoppingCartBundleProductItem](
	                        [Id] [int] IDENTITY(1,1) NOT NULL,
	                        [ShoppingCartItemId] [int] NOT NULL,
	                        [ParentProductId] [int] NOT NULL,
	                        [AssociatedProductId] [int] NOT NULL,
	                        [Quantity] [int] NOT NULL,
	                        [Price] DECIMAL(18,4) NOT NULL,
    PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, 
	IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, 
    ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
 ELSE
    BEGIN
	    SELECT 'ShoppingCartBundleProductItem - Table already exists in this database.'
    END
 GO

 /************************************************************************************************************/

 IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'AssociatedProductName')
	)
	BEGIN
			ALTER TABLE [ShoppingCartBundleProductItem] ADD	[AssociatedProductName] [nvarchar] (max) NULL 
   CONSTRAINT DF_ShoppingCartBundleProductItem_AssociatedProductName  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'AssociatedProductName - Column already exists in Product table.'
END
GO

/************************************************************************************************************/

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'AssociatedProductName')
	)
 BEGIN
			UPDATE [ShoppingCartBundleProductItem] SET 	[AssociatedProductName] = '' 
	END

GO
/************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'AssociatedProductTaxCategoryId')
	)
	BEGIN
			ALTER TABLE [ShoppingCartBundleProductItem] ADD	[AssociatedProductTaxCategoryId] [int] NULL 
			CONSTRAINT DF_ShoppingCartBundleProductItem_AssociatedProductTaxCategoryId  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'AssociatedProductTaxCategoryId - Column already exists in Product table.'
END
GO

/************************************************************************************************************/

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'AssociatedProductTaxCategoryId')
	)
 BEGIN
			UPDATE [ShoppingCartBundleProductItem] SET 	[AssociatedProductTaxCategoryId] = 0
	END

GO

/************************************************************************************************************/



IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'OrderId')
	)
	BEGIN
			ALTER TABLE [ShoppingCartBundleProductItem] ADD	[OrderId] [int] NULL 
			CONSTRAINT DF_ShoppingCartBundleProductItem_OrderId  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'OrderId - Column already exists in ShoppingCartBundleProductItem table.'
END
GO

/************************************************************************************************************/

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'OrderId')
	)
 BEGIN
			UPDATE [ShoppingCartBundleProductItem] SET 	[OrderId] = 0
	END

GO
/************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'OrderItemId')
	)
	BEGIN
			ALTER TABLE [ShoppingCartBundleProductItem] ADD	[OrderItemId] [int] NULL 
			CONSTRAINT DF_ShoppingCartBundleProductItem_OrderItemId  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'OrderItemId - Column already exists in ShoppingCartBundleProductItem table.'
END
GO

/************************************************************************************************************/

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartBundleProductItem') 
		AND (sys.columns.[name] LIKE 'OrderItemId')
	)
 BEGIN
			UPDATE [ShoppingCartBundleProductItem] SET 	[OrderItemId] = 0
	END

GO


/************************************************************************************************************/

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'SAPOrderProcess') 
		AND (sys.columns.[name] LIKE 'ProductId')
	)
	BEGIN
			ALTER TABLE SAPOrderProcess
			DROP COLUMN [ProductId];
	END
ELSE
BEGIN
	SELECT 'ProductId - Column doesnot exists in SAPOrderProcess table.'
END
GO

/********************************************************************************************************************/

 IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'SAPOrderProcess') 
		AND (sys.columns.[name] LIKE 'IsBundle')
	)
	BEGIN
			ALTER TABLE SAPOrderProcess
			DROP COLUMN [IsBundle];
	END
ELSE
BEGIN
	SELECT 'IsBundle - Column doesnot exists in SAPOrderProcess table.'
END
GO
/********************************************************************************************************************/
