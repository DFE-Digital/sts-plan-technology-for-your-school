# Get recommendation text from database

1. Run `contentful/copy-to-database/get-content.sh` A new script will be created in
   `contentful/copy-to-database/` named `1.insert-content-<date>.sql` when it completes.

2. While that's working, ensure that `dbo.getContentfulUrl`, `dbo.contentfulRichTextNode`, and
   `dbo.contentfulRichTextToExcel` exist on the target server. Creation scripts can be found in
   `20261109_1845_AddRecommendationRichTextParsingScripts.sql`.

3. Copy and paste the contents of the following files into a T-SQL query window and run them in
   order.
   - `contentful/copy-to-database/1.insert-content-<date>.sql`
   - `contentful/copy-to-database/2.create-tables.sql`
   - `contentful/copy-to-database/3.populate-tables.sql`

4. Run the following script:

   ```sql
   DECLARE @contentfulEnvironment VARCHAR(256) = 'work_in_progress';

   SELECT
     cr.internalName AS [Internal Name],
     dbo.getContentfulUrl(@contentfulEnvironment, cr.contentfulRef) AS [Recommendation URL],
     cr.header AS [Header],
     cr.slug AS [Slug],
     dbo.contentfulRichTextToExcel(ctb.richText, 0) AS [Recommendation text]
   FROM
     contentful.recommendation cr
     JOIN contentful.textBody ctb ON ctb.contentfulRef = cr.textBodyRef
   -- Edit or remove the filter
   -- WHERE
   --   cr.internalName LIKE 'CORE%'
   ;
   ```
