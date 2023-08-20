
/*************************************************************************************************************
Script			: Alter table Store - add column With Default Values
Purpose			: To Add the IsTieredShippingEnabled in the Store table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
--------------------------------------
IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Store') 
		        AND (sys.columns.[name] LIKE 'IsTieredShippingEnabled')
	)
	BEGIN
	   ALTER TABLE [Store] ADD [IsTieredShippingEnabled] [bit] NULL 
    CONSTRAINT DF_Store_IsTieredShippingEnabled  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsTieredShippingEnabled - Column already exists in Store table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Store') 
		      AND (sys.columns.[name] LIKE 'IsTieredShippingEnabled')
	)
 BEGIN
    UPDATE [Store] SET 	[IsTieredShippingEnabled] = 0 
	END
GO

Insert into LocaleStringResource 
values (1,'Admin.Configuration.Stores.Fields.IsTieredShippingEnabled', 'Enable Tiered Shipping')
Insert into LocaleStringResource 
values (2,'Admin.Configuration.Stores.Fields.IsTieredShippingEnabled', 'Enable Tiered Shipping')
Insert into LocaleStringResource 
values (3,'Admin.Configuration.Stores.Fields.IsTieredShippingEnabled', 'Enable Tiered Shipping')

--------------------------------------------------------------------------------------------
 /*************************************************************************************************************
Script			: Create table - StoreWiseTierShipping
Purpose			: To Create table - StoreWiseTierShipping
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		WHERE sys.objects.object_id = OBJECT_ID(N'StoreWiseTierShipping') 
	)
	BEGIN
			CREATE TABLE [dbo].[StoreWiseTierShipping](
	                                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                                    [MinPrice] decimal(18,4) NOT NULL,
	                                    [MaxPrice] decimal(18,4) NOT NULL,
	                                    [ShippingAmount] decimal(18,4) NOT NULL,
										[StoreId] [int] NOT NULL,
   CONSTRAINT [PK_StoreWiseTierShipping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, 
IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, 
FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
	END

 GO

 
 ------------------------------------------------
 /*************************************************************************************************************
Script			: Alter table Store - add column With Default Values
Purpose			: To Add the IsContractShippingEnabled in the Store table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

 IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Store') 
		        AND (sys.columns.[name] LIKE 'IsContractShippingEnabled')
	)
	BEGIN
	   ALTER TABLE [Store] ADD [IsContractShippingEnabled] [bit] NULL 
    CONSTRAINT DF_Store_IsContractShippingEnabled  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsContractShippingEnabled - Column already exists in Store table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Store') 
		      AND (sys.columns.[name] LIKE 'IsContractShippingEnabled')
	)
 BEGIN
    UPDATE [Store] SET 	[IsContractShippingEnabled] = 0 
	END
GO

------
/*************************************************************************************************************
Script			: Alter table Store - add column With Default Values
Purpose			: To Add the IsInterOfficeDeliveryEnabled in the Store table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Store') 
		        AND (sys.columns.[name] LIKE 'IsInterOfficeDeliveryEnabled')
	)
	BEGIN
	   ALTER TABLE [Store] ADD [IsInterOfficeDeliveryEnabled] [bit] NULL 
    CONSTRAINT DF_Store_IsInterOfficeDeliveryEnabled  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsInterOfficeDeliveryEnabled - Column already exists in Store table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Store') 
		      AND (sys.columns.[name] LIKE 'IsInterOfficeDeliveryEnabled')
	)
 BEGIN
    UPDATE [Store] SET 	[IsInterOfficeDeliveryEnabled] = 0 
	END
GO


Insert into LocaleStringResource 
values (1,'Admin.Configuration.Stores.Fields.IsContractShippingEnabled', 'Enable Internal Billing for shipping')
Insert into LocaleStringResource 
values (2,'Admin.Configuration.Stores.Fields.IsContractShippingEnabled', 'Enable Internal Billing for shipping')
Insert into LocaleStringResource 
values (3,'Admin.Configuration.Stores.Fields.IsContractShippingEnabled', 'Enable Internal Billing for shipping')

Insert into LocaleStringResource 
values (1,'Admin.Configuration.Stores.Fields.IsInterOfficeDeliveryEnabled', 'Enable Inter-Office Delivery')
Insert into LocaleStringResource 
values (2,'Admin.Configuration.Stores.Fields.IsInterOfficeDeliveryEnabled', 'Enable Inter-Office Delivery')
Insert into LocaleStringResource 
values (3,'Admin.Configuration.Stores.Fields.IsInterOfficeDeliveryEnabled', 'Enable Inter-Office Delivery')


---------------
/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the IsTieredShippingEnabled in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'IsTieredShippingEnabled')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [IsTieredShippingEnabled] [bit] NULL 
    CONSTRAINT DF_ShoppingCartItem_IsTieredShippingEnabled  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsTieredShippingEnabled - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'IsTieredShippingEnabled')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[IsTieredShippingEnabled] = 0 
	END
GO

-------------------------------------
 /*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the IsContractShippingEnabled in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

 IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'IsContractShippingEnabled')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [IsContractShippingEnabled] [bit] NULL 
    CONSTRAINT DF_ShoppingCartItem_IsContractShippingEnabled  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsContractShippingEnabled - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'IsContractShippingEnabled')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[IsContractShippingEnabled] = 0 
	END
GO

-------------------------------------
/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the IsInterOfficeDeliveryEnabled in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'IsInterOfficeDeliveryEnabled')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [IsInterOfficeDeliveryEnabled] [bit] NULL 
    CONSTRAINT DF_ShoppingCartItem_IsInterOfficeDeliveryEnabled  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsInterOfficeDeliveryEnabled - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'IsInterOfficeDeliveryEnabled')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[IsInterOfficeDeliveryEnabled] = 0 
	END
GO

/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the FlatShipping in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'FlatShipping')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [FlatShipping] decimal(18,4) NULL 
    CONSTRAINT DF_ShoppingCartItem_FlatShipping  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'FlatShipping - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'FlatShipping')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[FlatShipping] = 0 
	END
GO

/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the RCNumber in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'RCNumber')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [RCNumber] nvarchar(100) NULL 
    CONSTRAINT DF_ShoppingCartItem_RCNumber  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'RCNumber - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'RCNumber')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[RCNumber] = ''
	END
GO

/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the MailStopAddress in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'MailStopAddress')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [MailStopAddress] nvarchar(100) NULL 
    CONSTRAINT DF_ShoppingCartItem_MailStopAddress  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'MailStopAddress - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'MailStopAddress')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[MailStopAddress] = ''
	END
GO

/*************************************************************************************************************
Script			: Alter table OrderItem - add column With Default Values
Purpose			: To Add the MailStopAddress in the OrderItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		        AND (sys.columns.[name] LIKE 'MailStopAddress')
	)
	BEGIN
	   ALTER TABLE [OrderItem] ADD [MailStopAddress] nvarchar(100) NULL 
    CONSTRAINT DF_OrderItem_MailStopAddress  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'MailStopAddress - Column already exists in OrderItem table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		      AND (sys.columns.[name] LIKE 'MailStopAddress')
	)
 BEGIN
    UPDATE [OrderItem] SET 	[MailStopAddress] = ''
	END
GO
---------------------------------------------------------------------------------------------------------
/*************************************************************************************************************
Script			: Alter table OrderItem - add column With Default Values
Purpose			: To Add the RCNumber in the OrderItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		        AND (sys.columns.[name] LIKE 'RCNumber')
	)
	BEGIN
	   ALTER TABLE [OrderItem] ADD [RCNumber] nvarchar(100) NULL 
    CONSTRAINT DF_OrderItem_RCNumber  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'RCNumber - Column already exists in OrderItem table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		      AND (sys.columns.[name] LIKE 'RCNumber')
	)
 BEGIN
    UPDATE [OrderItem] SET 	[RCNumber] = ''
	END
GO

/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the IsFirstCartItem in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		        AND (sys.columns.[name] LIKE 'IsFirstCartItem')
	)
	BEGIN
	   ALTER TABLE [ShoppingCartItem] ADD [IsFirstCartItem] [bit] NULL 
    CONSTRAINT DF_ShoppingCartItem_IsFirstCartItem  DEFAULT(0)
	END
ELSE
BEGIN
	SELECT 'IsFirstCartItem - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		      AND (sys.columns.[name] LIKE 'IsFirstCartItem')
	)
 BEGIN
    UPDATE [ShoppingCartItem] SET 	[IsFirstCartItem] = 0 
	END
GO

/*************************************************************************************************************
Script			: Alter table Order - add column With Default Values
Purpose			: To Add the MailStopAddress in the Order table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Order') 
		        AND (sys.columns.[name] LIKE 'MailStopAddress')
	)
	BEGIN
	   ALTER TABLE [Order] ADD [MailStopAddress] nvarchar(100) NULL 
    CONSTRAINT DF_Order_MailStopAddress  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'MailStopAddress - Column already exists in Order table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Order') 
		      AND (sys.columns.[name] LIKE 'MailStopAddress')
	)
 BEGIN
    UPDATE [Order] SET 	[MailStopAddress] = ''
	END
GO


/*************************************************************************************************************
Script			: Alter table Order - add column With Default Values
Purpose			: To Add the RCNumber in the Order table
Developed By	: Cognizant Technology Solutions
Date			: October 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Order') 
		        AND (sys.columns.[name] LIKE 'RCNumber')
	)
	BEGIN
	   ALTER TABLE [Order] ADD [RCNumber] nvarchar(100) NULL 
    CONSTRAINT DF_Order_RCNumber  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'RCNumber - Column already exists in Order table.'
END
GO

IF EXISTS(
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Order') 
		      AND (sys.columns.[name] LIKE 'RCNumber')
	)
 BEGIN
    UPDATE [Order] SET 	[RCNumber] = ''
	END
GO

Insert into LocaleStringResource 
values (1,'Admin.Orders.Fields.RCNumber', 'RC Number')
Insert into LocaleStringResource 
values (2,'Admin.Orders.Fields.RCNumber', 'RC Number')
Insert into LocaleStringResource 
values (3,'Admin.Orders.Fields.RCNumber', 'RC Number')
Insert into LocaleStringResource 
values (1,'Admin.Orders.Fields.MailStopAddress', 'Mail Stop Address')
Insert into LocaleStringResource 
values (2,'Admin.Orders.Fields.MailStopAddress', 'Mail Stop Address')
Insert into LocaleStringResource 
values (3,'Admin.Orders.Fields.MailStopAddress', 'Mail Stop Address')