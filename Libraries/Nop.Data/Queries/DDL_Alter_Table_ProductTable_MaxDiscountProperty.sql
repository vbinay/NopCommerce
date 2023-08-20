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
		AND (sys.columns.[name] LIKE 'MaxDiscountQuantity')
		
	)
	BEGIN
			ALTER TABLE Product
			ADD	
			MaxDiscountQuantity int 
			
			ALTER TABLE Product
			ADD CONSTRAINT PRD_MaxDiscountQty 
			DEFAULT 0 FOR MaxDiscountQuantity;

			UPDATE PRODUCT SET MaxDiscountQuantity=0

			

	END
ELSE
BEGIN
	SELECT 'column aleady exists in Product table'
END

