use nopcommerce_prod_38_06_23_2020
/*************************************************************************************************************
Script			: Alter table Product
				  add column With Default Values
Purpose			: To Add the MaxDiscountQuantity in the Product table
Developed By	: Cognizant Technology Solutions
Date			: 24 - Aug - 2020
*************************************************************************************************************/
	
IF NOT EXISTS(			
		SELECT 1
		FROM		sys.objects 
		INNER JOIN	sys.columns 
		ON			sys.objects.object_id = sys.columns.object_id 
		WHERE sys.objects.object_id = OBJECT_ID(N'Product') 
		AND (sys.columns.[name] LIKE 'RequireOtherproductsOption')
		
	)
	BEGIN
			ALTER TABLE Product
			ADD	
			RequireOtherproductsOption varchar(50) 
			
			ALTER TABLE Product
			ADD CONSTRAINT PRD_RequireOtherproductsOption 
			DEFAULT 'OR' FOR RequireOtherproductsOption;

	END
ELSE
BEGIN
	SELECT 'column aleady exists in Product table'
END

