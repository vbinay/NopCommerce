
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].updateEmail
(
	-- Add the parameters for the function here
	@Email VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @ret VARCHAR(MAX)
	
	DECLARE @account VARCHAR(MAX)
	SET @account = LEFT(@Email, CHARINDEX('@', @Email))
	
	SET @ret = @account + 'sodexomyway.net'
	
	RETURN @ret
END