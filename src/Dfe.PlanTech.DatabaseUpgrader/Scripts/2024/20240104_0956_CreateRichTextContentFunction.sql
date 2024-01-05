BEGIN TRAN

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
      JOIN  cte t
      ON c.ParentId = t.Id 
    )
    
    SELECT * FROM cte
  )  

  GO;
  
  CREATE TYPE IdTableType
   AS TABLE( Id BIGINT);
  
  GO;

  CREATE FUNCTION [Contentful].[SelectAllRichTextContentForParentIds] (
      @ParentIds IdTableType READONLY
  )
  RETURNS @T table(Id BIGINT, [Value] NVARCHAR(MAX), NodeType NVARCHAR(MAX), DataId BIGINT, parentid BIGINT)
  BEGIN
    DECLARE @idColumn INT

    SELECT @idColumn = min(Id) FROM @ParentIds

    WHILE @idColumn IS NOT NULL
      BEGIN
        INSERT INTO @T SELECT * FROM [Contentful].[SelectAllRichTextContentForParentId](@idColumn)
        SELECT @idColumn = min(Id) FROM @ParentIds WHERE Id > @idColumn

    END

		RETURN 
  END

  GO;
  
COMMIT TRAN