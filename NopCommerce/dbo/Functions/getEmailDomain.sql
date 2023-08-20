
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].getEmailDomain
(
	-- Add the parameters for the function here
	@Email VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @ret VARCHAR(MAX)
	SET @ret = RIGHT(@Email, LEN(@Email) - CHARINDEX('@', @Email))
	
	RETURN @ret
END