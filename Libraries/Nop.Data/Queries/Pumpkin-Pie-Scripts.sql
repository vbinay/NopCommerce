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
			CONSTRAINT DF_Product_ReservationTimeInterval  DEFAULT(0)
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
		      AND (sys.columns.[name] LIKE 'ReservationTimeInterval')
	)
 BEGIN
    UPDATE [Product] SET 	[ReservationTimeInterval] = 0
END
GO