ALTER FUNCTION [Contentful].[SelectAllRichTextContentForPageSlug] (
    @PageSlug NVARCHAR(MAX)
)
RETURNS @ReturnTable table(Id BIGINT, [Value] NVARCHAR(MAX), NodeType NVARCHAR(MAX), DataId BIGINT, ParentId BIGINT)
BEGIN
  DECLARE @ContentComponentIds TABLE (Id NVARCHAR(MAX))
  DECLARE @RichTextIds TABLE (Id bigint)

  --Get content components for the page
  INSERT INTO @ContentComponentIds
    SELECT ISNULL(PC.BeforeContentComponentId, PC.ContentComponentId) AS ContentComponentId
    FROM [Contentful].[PageContents] PC
    LEFT JOIN [Contentful].[Pages] P ON PC.PageId = P.Id
    WHERE P.Slug = @PageSlug

  --Get rich text ids for text bodies and warnings
  INSERT INTO @RichTextIds
      SELECT DISTINCT TB.RichTextId
      FROM [Contentful].[TextBodies] AS TB
      JOIN @ContentComponentIds AS CC ON TB.Id = CC.Id
    UNION
      SELECT DISTINCT TB.RichTextId
      FROM [Contentful].[Warnings] AS W
      LEFT JOIN [Contentful].[TextBodies] AS TB ON W.TextId = TB.ID
      JOIN @ContentComponentIds AS CC ON W.Id = CC.Id

  --For each rich text id, execute the SelectAllRichTextContentForParentId function and add to the return table
  DECLARE @idColumn BIGINT

  SELECT @idColumn = min(Id) FROM @RichTextIds

  WHILE @idColumn IS NOT NULL
    BEGIN
      INSERT INTO @ReturnTable SELECT [Id], [Value], [NodeType], [DataId], [ParentId] FROM [Contentful].[SelectAllRichTextContentForParentId](@idColumn)
      SELECT @idColumn = min(Id) FROM @RichTextIds WHERE Id > @idColumn
    END

  RETURN
END
