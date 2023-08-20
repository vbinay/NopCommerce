

CREATE PROCEDURE [LanguagePackImport]
(
	@LanguageId int,
	@XmlPackage xml
)
AS
BEGIN
	IF EXISTS(SELECT * FROM [Language] WHERE [Id] = @LanguageId)
	BEGIN
		CREATE TABLE #LocaleStringResourceTmp
			(
				[LanguageId] [int] NOT NULL,
				[ResourceName] [nvarchar](200) NOT NULL,
				[ResourceValue] [nvarchar](MAX) NOT NULL
			)

		INSERT INTO #LocaleStringResourceTmp (LanguageID, ResourceName, ResourceValue)
		SELECT	@LanguageId, nref.value('@Name', 'nvarchar(200)'), nref.value('Value[1]', 'nvarchar(MAX)')
		FROM	@XmlPackage.nodes('//Language/LocaleResource') AS R(nref)

		DECLARE @ResourceName nvarchar(200)
		DECLARE @ResourceValue nvarchar(MAX)
		DECLARE cur_localeresource CURSOR FOR
		SELECT LanguageID, ResourceName, ResourceValue
		FROM #LocaleStringResourceTmp
		OPEN cur_localeresource
		FETCH NEXT FROM cur_localeresource INTO @LanguageId, @ResourceName, @ResourceValue
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF (EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE LanguageID=@LanguageId AND ResourceName=@ResourceName))
			BEGIN
				UPDATE [LocaleStringResource]
				SET [ResourceValue]=@ResourceValue
				WHERE LanguageID=@LanguageId AND ResourceName=@ResourceName
			END
			ELSE 
			BEGIN
				INSERT INTO [LocaleStringResource]
				(
					[LanguageId],
					[ResourceName],
					[ResourceValue]
				)
				VALUES
				(
					@LanguageId,
					@ResourceName,
					@ResourceValue
				)
			END
			
			
			FETCH NEXT FROM cur_localeresource INTO @LanguageId, @ResourceName, @ResourceValue
			END
		CLOSE cur_localeresource
		DEALLOCATE cur_localeresource

		DROP TABLE #LocaleStringResourceTmp
	END
END