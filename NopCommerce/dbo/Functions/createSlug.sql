-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION createSlug
(
	-- Add the parameters for the function here
	@EntityName VARCHAR(MAX), @Slug VARCHAR(MAX), @Index INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @ret VARCHAR(MAX)
	SET @ret = dbo.urlencode(@Slug)
	
	DECLARE @SlugIndex VARCHAR(MAX)
	
	DECLARE @rows INT
	SET @rows = ( SELECT COUNT(*) FROM [nopcommerce].[dbo].[UrlRecord] WHERE EntityName = @EntityName AND Slug = @ret )
	
	WHILE( @rows > 0 )
	BEGIN
		SET @Index = @Index + 1
		SET @SlugIndex = CAST( @Index AS VARCHAR(MAX) )
		
		SET @ret = dbo.urlencode(@Slug) + '-' + @SlugIndex
		
		SET @rows = ( SELECT COUNT(*) FROM [nopcommerce].[dbo].[UrlRecord] WHERE EntityName = @EntityName AND Slug = @ret )
	END

	RETURN @ret
END