
/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the Reservation ticket cap per slot in the Product table
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
		        AND (sys.columns.[name] LIKE 'ReservationCapPerSlot')
	)
	BEGIN
		ALTER TABLE [Product] ADD	[ReservationCapPerSlot] [int] NULL   
		CONSTRAINT DF_Product_ReservationCapPerSlot  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'ReservationCapPerSlot - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		      AND (sys.columns.[name] LIKE 'ReservationCapPerSlot')
	)
 BEGIN
    UPDATE [Product] SET 	[ReservationCapPerSlot] = 0
	END

GO

/*************************************************************************************************************
Product Table Changes
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the IsReservation in the Product table
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
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
	BEGIN
			ALTER TABLE [Product] ADD [IsReservation] [bit] NULL 
			CONSTRAINT DF_Product_IsReservation  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsReservation - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
 BEGIN
			UPDATE [Product] SET 	[IsReservation] = 0 
	END

GO

/*************************************************************************************************************
Product Table Changes
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the ReservationTimeInterval in the Product table
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
		AND (sys.columns.[name] LIKE 'ReservationTimeInterval')
	)
	BEGIN
			ALTER TABLE [Product] ADD ReservationTimeInterval int NULL 
			CONSTRAINT DF_Product_IsReservation  DEFAULT(0)
	END
ELSE
BEGIN
	SELECT 'ReservationTimeInterval - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
 BEGIN
			UPDATE [Product] SET 	[IsReservation] = 0 
	END

GO
---------------------------------------

/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the MaxWindowDays in the Product table
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
		        AND (sys.columns.[name] LIKE 'MaxWindowDays')
	)
	BEGIN
		ALTER TABLE [Product] ADD	[MaxWindowDays] [int] NULL   
		CONSTRAINT DF_Product_MaxWindowDays  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'MaxWindowDays - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		      AND (sys.columns.[name] LIKE 'MaxWindowDays')
	)
 BEGIN
    UPDATE [Product] SET 	[MaxWindowDays] = 0
	END

GO
---------------------------------------------------------------


/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the ReservationStartTime in the Product table
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
		AND (sys.columns.[name] LIKE 'ReservationStartTime')
	)
	BEGIN
			ALTER TABLE [Product] ADD	[ReservationStartTime] [nvarchar] (max) NULL 
   CONSTRAINT DF_Product_ReservationStartTime  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'ReservationStartTime - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'ReservationStartTime')
	)
 BEGIN
			UPDATE [Product] SET 	[ReservationStartTime] = '' 
	END

GO
--------------------------------------------
/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the ReservationEndTime in the Product table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON		sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'ReservationEndTime')
	)
	BEGIN
		ALTER TABLE [Product] ADD	[ReservationEndTime] [nvarchar] (max) NULL 
		CONSTRAINT DF_Product_ReservationEndTime  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'ReservationEndTime - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'ReservationEndTime')
	)
 BEGIN
			UPDATE [Product] SET 	[ReservationEndTime] = ''
	END

GO
--------------------------------------------------------------------------------------------

/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the MaxOccupancy in the Product table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		        AND (sys.columns.[name] LIKE 'MaxOccupancy')
	)
	BEGIN
		ALTER TABLE [Product] ADD	[MaxOccupancy] [int] NULL   
		CONSTRAINT DF_Product_MaxOccupancy  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'MaxOccupancy - Column already exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		      AND (sys.columns.[name] LIKE 'MaxOccupancy')
	)
 BEGIN
    UPDATE [Product] SET 	[MaxOccupancy] = 0
	END

GO

/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the ReservationInterval in the Product table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		        AND (sys.columns.[name] LIKE 'ReservationInterval')
	)
	BEGIN
		ALTER TABLE [Product] ADD	[ReservationInterval] [int] NULL  
		CONSTRAINT DF_Product_ReservationInterval  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'ReservationInterval - Column aleady exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'ReservationInterval')
	)
 BEGIN
    	UPDATE [Product] SET 	[ReservationInterval] = 0
	END

GO
---------------------------------------
/*************************************************************************************************************
Script			: Alter table Product - add column With Default Values
Purpose			: To Add the LeadTime in the Product table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		        AND (sys.columns.[name] LIKE 'LeadTime')
	)
	BEGIN
		ALTER TABLE [Product] ADD	[LeadTime] [int] NULL  
		CONSTRAINT DF_Product_LeadTime  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'LeadTime - Column aleady exists in Product table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'LeadTime')
	)
 BEGIN
    	UPDATE [Product] SET 	[LeadTime] = 0
	END

GO

----------------------------------------------------------------------
/*************************************************************************************************************
ShoppingCartItem Table Changes

Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the IsReservation in the ShoppingCartItem table
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
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
	BEGIN
			ALTER TABLE [ShoppingCartItem] ADD [IsReservation] [bit] NULL 
			CONSTRAINT DF_ShoppingCartItem_IsReservation  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsReservation - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
 BEGIN
			UPDATE [ShoppingCartItem] SET 	[IsReservation] = 0 
	END

GO

/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the ReservedTimeSlot in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'ReservedTimeSlot')
	)
	BEGIN
			ALTER TABLE [ShoppingCartItem] ADD [ReservedTimeSlot] [nvarchar] (max) NULL 
			CONSTRAINT DF_ShoppingCartItem_ReservedTimeSlot  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'ReservedTimeSlot - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'ReservedTimeSlot')
	)
 BEGIN
			UPDATE [ShoppingCartItem] SET 	[ReservedTimeSlot] ='' 
	END

GO

/*************************************************************************************************************
Script			: Alter table ShoppingCartItem - add column With Default Values
Purpose			: To Add the ReservationDate in the ShoppingCartItem table
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'ReservationDate')
	)
	BEGIN
			ALTER TABLE [ShoppingCartItem] ADD [ReservationDate] [DateTime] NULL 
			CONSTRAINT DF_ShoppingCartItem_ReservationDate  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'ReservationDate - Column already exists in ShoppingCartItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'ShoppingCartItem') 
		AND (sys.columns.[name] LIKE 'ReservationDate')
	)
 BEGIN
			UPDATE [ShoppingCartItem] SET 	[ReservationDate] = GetDate()
 END

GO

---------------------------------------------------------------

/*************************************************************************************************************
OrderItem Table Changes

Script			: Alter table OrderItem  - add column With Default Values
Purpose			: To Add the IsReservation in the Product table
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
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
	BEGIN
			ALTER TABLE [OrderItem] ADD [IsReservation] [bit] NULL 
			CONSTRAINT DF_OrderItem_IsReservation  DEFAULT(NULL)
	END
ELSE
BEGIN
	SELECT 'IsReservation - Column already exists in OrderItem table.'
END
GO

IF EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'OrderItem') 
		AND (sys.columns.[name] LIKE 'IsReservation')
	)
 BEGIN
			UPDATE [OrderItem] SET 	[IsReservation] = 0 
	END

GO

--------------------------------------------------------------------------------------------
/*************************************************************************************************************
Create Table Reservation Products

Script			: Create Table Reservation Products
Purpose			: Create Table Reservation Products
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		WHERE sys.objects.object_id = OBJECT_ID(N'ReservationProducts') 
	)
	BEGIN
			CREATE TABLE [dbo].[ReservationProducts](
	                                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                                    [ProductId] [int] NULL,
	                                    [TimeslotsConfigured] [nvarchar](max) NULL,
	                                    [OccupancyUnitsAvailable] [int] NULL,
   CONSTRAINT [PK_ReservationProducts] PRIMARY KEY CLUSTERED 
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
Create Table ProductFulfillmentCalendar

Script			: Create Table ProductFulfillmentCalendar 
Purpose			: Create Table ProductFulfillmentCalendar
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		WHERE sys.objects.object_id = OBJECT_ID(N'ProductFulfillmentCalendar') 
	)
	BEGIN
	CREATE TABLE [dbo].[ProductFulfillmentCalendar](
	                        [Id] [int] IDENTITY(1,1) NOT NULL,
	                        [ProductId] [int] NOT NULL,
	                        [Date] [datetime] NOT NULL
 CONSTRAINT [PK_ProductFulfillmentCalendar] PRIMARY KEY CLUSTERED 
                        (
	                        [Id] ASC
                        ) 
                        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, 
                        IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, 
                        FILLFACTOR = 90) ON [PRIMARY]
 ) ON [PRIMARY]


ALTER TABLE [dbo].[ProductFulfillmentCalendar]  WITH CHECK 
ADD  CONSTRAINT [ProductFulfillmentCalendar_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([Id])

ALTER TABLE [dbo].[ProductFulfillmentCalendar] 
CHECK CONSTRAINT [ProductFulfillmentCalendar_Product]
	END
 ELSE
    BEGIN
	    SELECT 'ProductFulfillmentCalendar - Table already exists in this database.'
    END
 GO
 
 -------------------------------------------------------------------
 
 /*************************************************************************************************************
Create Table ReservedProduct

Script			: Create Table ReservedProduct
Purpose			: Create Table ReservedProduct
Developed By	: Cognizant Technology Solutions
Date			: July 2020
Script Running Instruction - Please select statement till 'GO' in one run.
*************************************************************************************************************/

IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		WHERE sys.objects.object_id = OBJECT_ID(N'ReservedProduct') 
	)
	BEGIN
 CREATE TABLE [dbo].[ReservedProduct](
	                        [Id] [int] IDENTITY(1,1) NOT NULL,
	                        [ProductId] [int] NOT NULL,
	                        [OrderItemId] [int] NOT NULL,
	                        [ReservationDate] [DateTime] NOT NULL,
	                        [ReservedTimeslot] [varchar](255) NOT NULL,
	                        [ReservedUnits] [int] NOT NULL,
    PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, 
    ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
 ELSE
    BEGIN
	    SELECT 'ReservedProduct - Table already exists in this database.'
    END
 GO

 INSERT INTO [dbo].[LocaleStringResource] 
values (1,'Admin.Catalog.Products.IsReservation','Is Reservation')
INSERT INTO [dbo].[LocaleStringResource] 
values (2,'Admin.Catalog.Products.IsReservation','Is Reservation')
INSERT INTO [dbo].[LocaleStringResource] 
values (3,'Admin.Catalog.Products.IsReservation','Is Reservation')
 GO

INSERT INTO [dbo].[LocaleStringResource] 
values (1,'Admin.Reservations','Reservations')
INSERT INTO [dbo].[LocaleStringResource] 
values (2,'Admin.Reservations','Reservations')
INSERT INTO [dbo].[LocaleStringResource] 
values (3,'Admin.Reservations','Reservations')
 GO

insert into [dbo].[LocaleStringResource]
values (1,'Admin.Orders.List.OrderId','Order Id')
insert into [dbo].[LocaleStringResource]
values (2,'Admin.Orders.List.OrderId','Order Id')
insert into [dbo].[LocaleStringResource]
values (3,'Admin.Orders.List.OrderId','Order Id')

insert into [dbo].[LocaleStringResource]
values (1,'Admin.Orders.List.OrderId','Order Id')
insert into [dbo].[LocaleStringResource]
values (2,'Admin.Orders.List.OrderId','Order Id')
insert into [dbo].[LocaleStringResource]
values (3,'Admin.Orders.List.OrderId','Order Id')

GO