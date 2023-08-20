CREATE FUNCTION Parse_For_Domain_Name (
@url nvarchar(255)
)
returns nvarchar(255)

AS

BEGIN

declare @domain nvarchar(255)

-- Check if there is the "http://" in the @url
declare @http nvarchar(10)
declare @https nvarchar(10)
declare @protocol nvarchar(10)
set @http = 'http://'
set @https = 'https://'

declare @isHTTPS bit
set @isHTTPS = 0

select @domain = CharIndex(@http, @url)

if CharIndex(@http, @url) > 1
begin
if CharIndex(@https, @url) = 1
set @isHTTPS = 1
else
select @url = @http + @url
-- return 'Error at : ' + @url
-- select @url = substring(@url, CharIndex(@http, @url), len(@url) - CharIndex(@http, @url) + 1)
end

if CharIndex(@http, @url) = 0
if CharIndex(@https, @url) = 1
set @isHTTPS = 1
else
select @url = @http + @url

if @isHTTPS = 1
set @protocol = @https
else
set @protocol = @http

if CharIndex(@protocol, @url) = 1
begin
select @url = substring(@url, len(@protocol) + 1, len(@url)-len(@protocol))
if CharIndex('/', @url) > 0
select @url = substring(@url, 0, CharIndex('/', @url))

declare @i int
set @i = 0
while CharIndex('.', @url) > 0
begin
select @i = CharIndex('.', @url)
select @url = stuff(@url,@i,1,'/')
end
select @url = stuff(@url,@i,1,'.')

set @i = 0
while CharIndex('/', @url) > 0
begin
select @i = CharIndex('/', @url)
select @url = stuff(@url,@i,1,'.')
end

select @domain = substring(@url, @i + 1, len(@url)-@i)
end

return @domain

END