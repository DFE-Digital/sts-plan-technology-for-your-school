
CREATE FUNCTION [Contentful].[SelectAllRichTextContentForParentIds] (
    @ParentIds IdTableType READONLY
)
RETURNS @T table(Id BIGINT, [Value] NVARCHAR(MAX), NodeType NVARCHAR(MAX), DataId BIGINT, ParentId BIGINT)
BEGIN
  DECLARE @idColumn INT

  SELECT @idColumn = min(Id) FROM @ParentIds

  WHILE @idColumn IS NOT NULL
    BEGIN
      INSERT INTO @T SELECT [Id], [Value], [NodeType], [DataId], [ParentId] FROM [Contentful].[SelectAllRichTextContentForParentId](@idColumn)
      SELECT @idColumn = min(Id) FROM @ParentIds WHERE Id > @idColumn
    END

  RETURN
END
