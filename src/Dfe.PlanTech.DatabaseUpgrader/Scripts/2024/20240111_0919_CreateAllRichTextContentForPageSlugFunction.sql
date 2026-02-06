
CREATE FUNCTION [Contentful].[SelectAllRichTextContentForPageSlug] (
    @PageSlug NVARCHAR(MAX)
)
RETURNS @ReturnTable table(Id BIGINT, [Value] NVARCHAR(MAX), NodeType NVARCHAR(MAX), DataId BIGINT, ParentId BIGINT)
BEGIN
  DECLARE @ContentComponentIds TABLE (Id NVARCHAR(MAX))
  DECLARE @RichTextIds TABLE (Id NVARCHAR(MAX))

  INSERT INTO @ContentComponentIds
  SELECT CONCAT([BeforeContentComponentId], [ContentComponentId]) ContentComponentId FROM [Contentful].[PageContents] PC
  LEFT JOIN [Contentful].[Pages] P ON PC.PageId = P.Id
  WHERE P.Slug = @PageSlug

  INSERT INTO @RichTextIds
  SELECT RichTextId FROM [Contentful].[TextBodies] WHERE Id IN (SELECT * FROM @ContentComponentIds)
  UNION
  SELECT RichTextId FROM [Contentful].[Warnings] AS W
  LEFT JOIN [Contentful].[TextBodies] AS TB ON W.TextId = TB.ID
  WHERE W.Id IN (SELECT * FROM @ContentComponentIds)

  DECLARE @idColumn INT

  SELECT @idColumn = min(Id) FROM @RichTextIds

  WHILE @idColumn IS NOT NULL
    BEGIN
      INSERT INTO @ReturnTable SELECT [Id], [Value], [NodeType], [DataId], [ParentId] FROM [Contentful].[SelectAllRichTextContentForParentId](@idColumn)
      SELECT @idColumn = min(Id) FROM @RichTextIds WHERE Id > @idColumn
    END

  RETURN
END
