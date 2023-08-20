-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[updateStoreURL]
(
	-- Add the parameters for the function here
	@URL VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	SET @URL = REPLACE(@URL, 'https:', 'http:')
	SET @URL = REPLACE(@URL, '/shop', '/')
	SET @URL = REPLACE(@URL, '.sodexomyway.com', '-test.sodexomyway.net')
	SET @URL = REPLACE(@URL, '.sodexo.com', '-test.sodexomyway.net')

	RETURN @URL
END