IF NOT EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Address') 
		        AND (sys.columns.[name] LIKE 'RealId')
	)
	BEGIN
	   ALTER TABLE [Address] ADD [RealId] [nvarchar](max) NULL
		CONSTRAINT DF_Store_RealId  DEFAULT('')
	END
ELSE
BEGIN
	SELECT 'RealId - Column already exists in Address table.'
END
GO

IF EXISTS(			
		SELECT 1
		  FROM		sys.objects 
		INNER JOIN	sys.columns 
		  ON			sys.objects.object_id = sys.columns.object_id 
		  WHERE sys.objects.object_id = OBJECT_ID(N'Address') 
		      AND (sys.columns.[name] LIKE 'RealId')
	)
 BEGIN
    UPDATE [Address] SET [RealId] = ''
	END