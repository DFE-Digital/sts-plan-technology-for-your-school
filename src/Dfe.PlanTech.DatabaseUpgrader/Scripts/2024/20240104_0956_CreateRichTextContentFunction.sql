CREATE FUNCTION [Contentful].[SelectAllRichTextContentForParentId] (
    @ParentId int
)
RETURNS TABLE AS RETURN (
  WITH cte AS
  (
    SELECT Id, [Value], NodeType, DataId, ParentId FROM [Contentful].[RichTextContents] WHERE Id = @ParentId
    UNION ALL
    SELECT
      c.Id, c.[Value], c.NodeType, c.DataId, c.ParentId
    FROM
      [Contentful].[RichTextContents] c
    JOIN cte t
    ON c.ParentId = t.Id
  )

  SELECT [Id], [Value], [NodeType], [DataId], [ParentId] FROM cte
)
