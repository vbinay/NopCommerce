-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION urlencode 
(
	-- Add the parameters for the function here
	@text VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	SET @text = REPLACE(@text, ' ', '-')
	SET @text = REPLACE(@text, ';', '')
	SET @text = REPLACE(@text, '/', '')
	SET @text = REPLACE(@text, '?', '')
	SET @text = REPLACE(@text, ':', '')
	SET @text = REPLACE(@text, '@', '')
	SET @text = REPLACE(@text, '&', '')
	SET @text = REPLACE(@text, '=', '')
	SET @text = REPLACE(@text, '+', '')
	SET @text = REPLACE(@text, '$', '')
	SET @text = REPLACE(@text, ',', '')

	RETURN @text
END