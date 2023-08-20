
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TruncateTable] 
	-- Add the parameters for the stored procedure here
	@name NVARCHAR(MAX)
AS
BEGIN
	DECLARE @SQL NVARCHAR(MAX)
	
	EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
	 
	EXEC sp_MSForEachTable 'DISABLE TRIGGER ALL ON ?'
	 
	SET @SQL = 'SET QUOTED_IDENTIFIER ON; DELETE FROM '+@name
	EXECUTE sp_executesql @SQL
	 
	EXEC sp_MSForEachTable 'ENABLE TRIGGER ALL ON ?'
	 
	EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
	
	SET @SQL = 'IF(OBJECTPROPERTY(object_id('''+@name+'''), ''TableHasIdentity'') = 1) DBCC CHECKIDENT('''+@name+''', RESEED, 0)'
	EXECUTE sp_executesql @SQL
END