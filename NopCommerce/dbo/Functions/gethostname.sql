-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION gethostname
(
	-- Add the parameters for the function here
	@URL VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	IF CHARINDEX('http://',@URL) > 0 OR CHARINDEX('https://',@URL) > 0
	-- Ghetto-tastic
	SELECT @URL = REPLACE(@URL,'https://','')
	SELECT @URL = REPLACE(@URL,'http://','')
	SELECT @URL = REPLACE(@URL,'www','')
	-- Remove everything after "/" if one exists
	IF CHARINDEX('/',@URL) > 0 (SELECT @URL = LEFT(@URL,CHARINDEX('/',@URL)-1))

	-- Optional: Remove subdomains but differentiate between www.google.com and www.google.com.au
	SELECT @URL = PARSENAME(@URL,3) + '.' + PARSENAME(@URL,2) + '.' + PARSENAME(@URL,1)
	RETURN @URL
END