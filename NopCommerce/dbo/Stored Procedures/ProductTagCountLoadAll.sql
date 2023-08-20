

CREATE PROCEDURE [dbo].[ProductTagCountLoadAll]
(
	@StoreId int
)
AS
BEGIN
	SET NOCOUNT ON
	
	SELECT pt.Id as [ProductTagId], COUNT(p.Id) as [ProductCount]
	FROM ProductTag pt with (NOLOCK)
	LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK) ON pt.[Id] = pptm.[ProductTag_Id]
	LEFT JOIN Product p with (NOLOCK) ON pptm.[Product_Id] = p.[Id]
	WHERE
		p.[Deleted] = 0
		AND p.Published = 1
		AND (@StoreId = 0 or (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
			WHERE [sm].EntityId = p.Id AND [sm].EntityName = 'Product' and [sm].StoreId=@StoreId
			)))
	GROUP BY pt.Id
	ORDER BY pt.Id
END