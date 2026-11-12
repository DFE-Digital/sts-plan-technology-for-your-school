SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION dbo.getContentfulUrl
(
	@environment VARCHAR(256),
    @entryRef    VARCHAR(256)
)
RETURNS VARCHAR(256)
AS
BEGIN
    RETURN 'https://app.contentful.com/spaces/py5afvqdlxgo/environments/' + @environment + '/entries/' + @entryRef
END
GO


/*
  Converts Contentful rich text JSON into something that pastes into a single Excel cell.

  dbo.contentfulRichTextToExcel(@richText, @asHtml)

    @asHtml = 0  Plain text with line breaks, bullets and "link text (url)", wrapped in
                 double quotes so a copy from the SSMS grid lands in ONE Excel cell.
                 Needs: Tools > Options > Query Results > SQL Server > Results to Grid >
                 "Retain CR/LF on copy or save" (then open a new query window).

    @asHtml = 1  HTML fragment (bold headings, bold/italic/underline marks, styled links)
                 for use with Copy-RecommendationsToExcel.ps1, which puts it on the
                 clipboard as HTML so Excel keeps the formatting.
*/

CREATE OR ALTER FUNCTION dbo.contentfulRichTextNode
(
    @node      NVARCHAR(MAX),
    @asHtml    BIT,
    @listDepth INT
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @nl       NCHAR(1)       = NCHAR(10);
    DECLARE @type     NVARCHAR(50)   = JSON_VALUE(@node, '$.nodeType');
    DECLARE @text     NVARCHAR(MAX);
    DECLARE @open     NVARCHAR(MAX);
    DECLARE @close    NVARCHAR(MAX);
    DECLARE @children NVARCHAR(MAX);

    /* ---------- Leaf: text node (+ marks) ---------- */
    IF @type = N'text'
    BEGIN
        -- OPENJSON ... WITH rather than JSON_VALUE, which gives up on values > 4000 chars
        SET @text = (SELECT v FROM OPENJSON(@node) WITH (v NVARCHAR(MAX) '$.value'));

        IF @asHtml = 0
            RETURN @text;

        SET @text = REPLACE(REPLACE(REPLACE(@text, N'&', N'&amp;'), N'<', N'&lt;'), N'>', N'&gt;');

        -- Two statements: SQL Server won't allow ASC and DESC ordered aggregates in one SELECT (Msg 8711)
        DECLARE @marks TABLE (seq INT PRIMARY KEY, openTag NVARCHAR(100), closeTag NVARCHAR(20));

        INSERT @marks (seq, openTag, closeTag)
        SELECT CAST(m.[key] AS INT), t.openTag, t.closeTag
        FROM
            OPENJSON(@node, '$.marks') m
            JOIN (VALUES
                (N'bold',        N'<b>',   N'</b>'),
                (N'italic',      N'<i>',   N'</i>'),
                (N'underline',   N'<u>',   N'</u>'),
                (N'superscript', N'<sup>', N'</sup>'),
                (N'subscript',   N'<sub>', N'</sub>'),
                (N'code',        N'<span style="font-family:Consolas">', N'</span>')
            ) t (markType, openTag, closeTag) ON t.markType = JSON_VALUE(m.value, '$.type');

        SELECT @open  = STRING_AGG(openTag,  N'') WITHIN GROUP (ORDER BY seq)      FROM @marks;
        SELECT @close = STRING_AGG(closeTag, N'') WITHIN GROUP (ORDER BY seq DESC) FROM @marks;

        RETURN CONCAT(@open, @text, @close);
    END;

    /* ---------- Container nodes: render children ---------- */
    DECLARE @isList BIT           = IIF(@type IN (N'unordered-list', N'ordered-list'), 1, 0);
    DECLARE @depth  INT           = @listDepth + @isList;
    DECLARE @indentUnit NVARCHAR(20) = IIF(@asHtml = 1, N'&nbsp;&nbsp;&nbsp;', N'   ');
    DECLARE @indent NVARCHAR(400) = REPLICATE(@indentUnit, @depth - 1);
    DECLARE @sep    NVARCHAR(10)  = CASE @type
                                        WHEN N'table-row' THEN N' | '
                                        WHEN N'document'  THEN @nl      -- blank line between top-level blocks
                                        ELSE N''
                                    END;

    SELECT @children = STRING_AGG(CAST(CONCAT(
            CASE @type
                WHEN N'unordered-list' THEN CONCAT(@indent, N'• ')
                WHEN N'ordered-list'   THEN CONCAT(@indent, CAST(c.[key] AS INT) + 1, N'. ')
                -- Extra paragraphs inside a list item (after a nested list, say) line up under the item text
                WHEN N'list-item'      THEN IIF(CAST(c.[key] AS INT) > 0 AND JSON_VALUE(c.value, '$.nodeType') = N'paragraph',
                                                CONCAT(@indent, @indentUnit), NULL)
            END,
            CASE
                WHEN @type = N'table-row' THEN TRIM(@nl FROM dbo.contentfulRichTextNode(c.value, @asHtml, @depth))
                ELSE dbo.contentfulRichTextNode(c.value, @asHtml, @depth)
            END
        ) AS NVARCHAR(MAX)), @sep) WITHIN GROUP (ORDER BY CAST(c.[key] AS INT))
    FROM OPENJSON(@node, '$.content') c;

    DECLARE @uri      NVARCHAR(4000) = JSON_VALUE(@node, '$.data.uri');
    DECLARE @targetId NVARCHAR(200)  = JSON_VALUE(@node, '$.data.target.sys.id');

    IF @asHtml = 1
        SET @uri = REPLACE(REPLACE(REPLACE(@uri, N'&', N'&amp;'), N'<', N'&lt;'), N'>', N'&gt;');

    RETURN CASE
        WHEN @type = N'paragraph'
            THEN CONCAT(@children, @nl)

        WHEN @type LIKE N'heading-%'
            THEN CONCAT(IIF(@asHtml = 1, CONCAT(N'<b>', @children, N'</b>'), @children), @nl)

        WHEN @type = N'hyperlink'
            THEN CONCAT(
                    IIF(@asHtml = 1,
                        CONCAT(N'<span style="color:#1d70b8;text-decoration:underline">', @children, N'</span>'),
                        @children),
                    N' (', @uri, N')')

        WHEN @type IN (N'entry-hyperlink', N'asset-hyperlink')
            THEN CONCAT(@children, N' [', @type, N': ', @targetId, N']')

        WHEN @type IN (N'embedded-entry-block', N'embedded-asset-block')
            THEN CONCAT(N'[', @type, N': ', @targetId, N']', @nl)

        WHEN @type = N'embedded-entry-inline'
            THEN CONCAT(N'[', @type, N': ', @targetId, N']')

        WHEN @type = N'hr'
            THEN CONCAT(N'――――――――――', @nl)

        WHEN @type = N'table-row'
            THEN CONCAT(@children, @nl)

        WHEN @type = N'table-header-cell' AND @asHtml = 1
            THEN CONCAT(N'<b>', TRIM(@nl FROM @children), N'</b>')

        -- document, list-item, lists, blockquote, table, table-cell, anything unknown
        ELSE @children
    END;
END;
GO


CREATE OR ALTER FUNCTION dbo.contentfulRichTextToExcel
(
    @richText NVARCHAR(MAX),
    @asHtml   BIT
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    IF @richText IS NULL OR ISJSON(@richText) = 0
        RETURN @richText;

    DECLARE @nl NCHAR(1)      = NCHAR(10);
    DECLARE @s  NVARCHAR(MAX) = dbo.contentfulRichTextNode(@richText, @asHtml, 0);

    -- Collapse runs of blank lines (empty paragraphs etc.) and trim the ends
    WHILE CHARINDEX(@nl + @nl + @nl, @s) > 0
        SET @s = REPLACE(@s, @nl + @nl + @nl, @nl + @nl);

    SET @s = TRIM(@nl FROM @s);

    IF @asHtml = 1
        -- Plain <br> makes Excel split the cell into new rows; this keeps it in one cell
        SET @s = REPLACE(@s, @nl, N'<br style="mso-data-placement:same-cell">');
    ELSE
        -- Quoted so Excel treats embedded line breaks as part of one cell on paste
        SET @s = CONCAT(N'"', REPLACE(@s, N'"', N'""'), N'"');

    RETURN @s;
END;
GO
