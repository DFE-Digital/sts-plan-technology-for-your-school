SELECT T.name AS [TABLE NAME], SUM(I.rows) AS [ROWCOUNT]
FROM sys.tables AS T
INNER JOIN sys.sysindexes AS I ON T.object_id = I.id
WHERE SCHEMA_NAME(T.schema_id) = 'Contentful' 
    AND I.indid < 2 
    AND T.name NOT IN ('ContentComponents', 'ComponentDropDowns', 'RichTextDataDbEntity', 'PageContents', 'RichTextContents', 'RichTextMarkDbEntity')
GROUP BY T.name
ORDER BY [TABLE NAME] ASC