CREATE PROCEDURE DeleteRichTextContents
    @ContentId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    -- Create a temporary table to hold the data
    CREATE TABLE #RichTexts (
        Id NVARCHAR(255),
        DataId NVARCHAR(255),
        ParentId NVARCHAR(255)
    )

    -- Insert data into the temporary table based on the provided ContentId
    INSERT INTO #RichTexts (Id, DataId, ParentId)
    SELECT Id, DataId, ParentId
    FROM Contentful.RichTextContentIdsWithContentComponentId
    WHERE ContentId = @ContentId

    -- Delete rows from RichTextContents table based on the data in the temporary table
    DELETE RTC
    FROM [Contentful].[RichTextContents] RTC
    LEFT JOIN [Contentful].RichTextDataDbEntity RTD ON RTC.DataId = RTD.Id
    LEFT JOIN [Contentful].[RichTextMarkDbEntity] RTM ON RTM.RichTextContentDbEntityId = RTC.Id
    WHERE RTC.Id IN (SELECT Id FROM #RichTexts)

    -- Drop the temporary table
    DROP TABLE #RichTexts;
END
