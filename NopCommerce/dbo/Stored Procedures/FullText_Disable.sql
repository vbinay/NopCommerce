


CREATE PROCEDURE [FullText_Disable]
AS
BEGIN
	EXEC('
	--drop indexes
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[Product]''))
		DROP FULLTEXT INDEX ON [Product]
	')

	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[LocalizedProperty]''))
		DROP FULLTEXT INDEX ON [LocalizedProperty]
	')

	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = object_id(''[ProductTag]''))
		DROP FULLTEXT INDEX ON [ProductTag]
	')

	--drop catalog
	EXEC('
	IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = ''nopCommerceFullTextCatalog'')
		DROP FULLTEXT CATALOG [nopCommerceFullTextCatalog]
	')
END