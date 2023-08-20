USE [nopcommerce_prod_38_5_25_2021]
Go

--select * from  product where name like '%bite%'
	DECLARE @LoopCounter INT , @MaxProductId INT,@LoopCountForAtt INT,@i INT=0
	SELECT @LoopCounter = 26172 , @MaxProductId = 26172 from product --where name like '%bite - Candy%'

	DECLARE @LoopCounter_map INT , @MaxProductId_map INT
	SELECT @LoopCounter_map = MIN(id) , @MaxProductId_map = MAX(id) from product where name like '%bite - Grocery%'
	SELECT @LoopCountForAtt=count(*) from  product where name like '%bite - Grocery%'
--section to insert records into the productattributemapping table for creating the list in dropdown.
WHILE (@LoopCounter IS NOT NULL
    AND @LoopCounter <= @MaxProductId)
BEGIN  
		DECLARE @id INT,@productAttributeid INT
		SELECT @id = id,@productAttributeid=51  FROM product WHERE Id = @LoopCounter

		INSERT INTO PRODUCT_PRODUCTATTRIBUTE_MAPPING (ProductId, ProductAttributeId, TextPrompt, 
							IsRequired, AttributeControlTypeId, DisplayOrder,ValidationMinLength,ValidationMaxLength,
							ValidationFileAllowedExtensions,DefaultValue,ConditionAttributeXml,SiteProductVariantAttributeId) 
		VALUES (@id,@productAttributeid,'Salty Snacks',1,1,@LoopCounter+1,NULL,NULL,NULL,NULL,NULL,NULL) 
		SET @LoopCounter  = @LoopCounter  + 1  

		DECLARE @productAttributeMappingId INT
		SELECT @productAttributeMappingId= SCOPE_IDENTITY()
		PRINT @productAttributeMappingId

	--CREATING THE MAPPING VALUES FOR THE PRODUCTS
	WHILE (@i<=@LoopCountForAtt)
	
	BEGIN
		DECLARE @map_id INT,@map_productAttributeid INT,@productname NVARCHAR(100)
		SELECT   @map_id = id,@productname=[Name]  FROM product WHERE  name like '%bite - Grocery%' ORDER BY id 
		OFFSET @i+1 ROWS
         FETCH NEXT 1 ROWS ONLY

		INSERT INTO ProductAttributeValue(ProductAttributeMappingId,AttributeValueTypeId,AssociatedProductId,
									[Name],ColorSquaresRgb,ImageSquaresPictureId,
									PriceAdjustment,WeightAdjustment,Cost,Quantity,IsPreSelected,DisplayOrder,PictureId)
		VALUES(@productAttributeMappingId,10,@map_id,@productname,NULL,0,0,0,0,1,1,0,0)

		SET @i  = @i  + 1  
	END
END