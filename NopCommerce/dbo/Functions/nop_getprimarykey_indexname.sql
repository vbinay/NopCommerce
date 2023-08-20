


CREATE FUNCTION [dbo].[nop_getprimarykey_indexname]
(
    @table_name nvarchar(1000) = null
)
RETURNS nvarchar(1000)
AS
BEGIN
	DECLARE @index_name nvarchar(1000)

    SELECT @index_name = i.name
	FROM sys.tables AS tbl
	INNER JOIN sys.indexes AS i ON (i.index_id > 0 and i.is_hypothetical = 0) AND (i.object_id=tbl.object_id)
	WHERE (i.is_unique=1 and i.is_disabled=0) and (tbl.name=@table_name)

    RETURN @index_name
END