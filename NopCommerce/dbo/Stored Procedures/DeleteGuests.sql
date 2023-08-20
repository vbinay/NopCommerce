

--new stored procedure
CREATE PROCEDURE [dbo].[DeleteGuests]
(
	@OnlyWithoutShoppingCart bit = 1,
	@CreatedFromUtc datetime,
	@CreatedToUtc datetime,
	@TotalRecordsDeleted int = null OUTPUT
)
AS
BEGIN
	CREATE TABLE #tmp_guests (CustomerId int)
		
	INSERT #tmp_guests (CustomerId)
	SELECT [Id] FROM [Customer] c with (NOLOCK)
	WHERE
	--created from
	((@CreatedFromUtc is null) OR (c.[CreatedOnUtc] > @CreatedFromUtc))
	AND
	--created to
	((@CreatedToUtc is null) OR (c.[CreatedOnUtc] < @CreatedToUtc))
	AND
	--shopping cart items
	((@OnlyWithoutShoppingCart=0) OR (NOT EXISTS(SELECT 1 FROM [ShoppingCartItem] sci with (NOLOCK) inner join [Customer] with (NOLOCK) on sci.[CustomerId]=c.[Id])))
	AND
	--guests only
	(EXISTS(SELECT 1 FROM [Customer_CustomerRole_Mapping] ccrm with (NOLOCK) inner join [Customer] with (NOLOCK) on ccrm.[Customer_Id]=c.[Id] inner join [CustomerRole] cr with (NOLOCK) on cr.[Id]=ccrm.[CustomerRole_Id] WHERE cr.[SystemName] = N'Guests'))
	AND
	--no orders
	(NOT EXISTS(SELECT 1 FROM [Order] o with (NOLOCK) inner join [Customer] with (NOLOCK) on o.[CustomerId]=c.[Id]))
	AND
	--no blog comments
	(NOT EXISTS(SELECT 1 FROM [BlogComment] bc with (NOLOCK) inner join [Customer] with (NOLOCK) on bc.[CustomerId]=c.[Id]))
	AND
	--no news comments
	(NOT EXISTS(SELECT 1 FROM [NewsComment] nc  with (NOLOCK)inner join [Customer] with (NOLOCK) on nc.[CustomerId]=c.[Id]))
	AND
	--no product reviews
	(NOT EXISTS(SELECT 1 FROM [ProductReview] pr with (NOLOCK) inner join [Customer] with (NOLOCK) on pr.[CustomerId]=c.[Id]))
	AND
	--no product reviews helpfulness
	(NOT EXISTS(SELECT 1 FROM [ProductReviewHelpfulness] prh with (NOLOCK) inner join [Customer] with (NOLOCK) on prh.[CustomerId]=c.[Id]))
	AND
	--no poll voting
	(NOT EXISTS(SELECT 1 FROM [PollVotingRecord] pvr with (NOLOCK) inner join [Customer] with (NOLOCK) on pvr.[CustomerId]=c.[Id]))
	AND
	--no forum topics 
	(NOT EXISTS(SELECT 1 FROM [Forums_Topic] ft with (NOLOCK) inner join [Customer] with (NOLOCK) on ft.[CustomerId]=c.[Id]))
	AND
	--no forum posts 
	(NOT EXISTS(SELECT 1 FROM [Forums_Post] fp with (NOLOCK) inner join [Customer] with (NOLOCK) on fp.[CustomerId]=c.[Id]))
	AND
	--no system accounts
	(c.IsSystemAccount = 0)
	
	--delete guests
	DELETE [Customer]
	WHERE [Id] IN (SELECT [CustomerID] FROM #tmp_guests)
	
	--delete attributes
	DELETE [GenericAttribute]
	WHERE ([EntityID] IN (SELECT [CustomerID] FROM #tmp_guests))
	AND
	([KeyGroup] = N'Customer')
	
	--total records
	SELECT @TotalRecordsDeleted = COUNT(1) FROM #tmp_guests
	
	DROP TABLE #tmp_guests
END