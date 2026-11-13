 /* =====================================================================
    Daily-cached page visit report
    ---------------------------------------------------------------------
    reporting.RefreshPageVisits  - rebuilds reporting.pageVisits if it
                                   hasn't been rebuilt today.
                                   @force = 1 rebuilds regardless.
    reporting.GetPageVisits      - Refreshes if stale, then returns rows.
                                   Point Power BI at:
                                   EXEC reporting.GetPageVisits

    If the shape of the final SELECT changes, drop
    reporting.pageVisits and run EXEC reporting.RefreshPageVisits
    @force = 1 as an admin so it is recreated with the new columns.
    ===================================================================== */


 /* ---------------------------------------------------------------------
    One-off setup
    --------------------------------------------------------------------- */
IF SCHEMA_ID(N'reporting') IS NULL
    EXEC (N'CREATE SCHEMA reporting AUTHORIZATION dbo');
GO

IF OBJECT_ID(N'reporting.refreshLog') IS NULL
    CREATE TABLE reporting.refreshLog
    (
        tableName     sysname        NOT NULL CONSTRAINT PK_reporting_refreshLog PRIMARY KEY,
        refreshedDate date           NOT NULL,
        refreshedAt   datetimeoffset NOT NULL,
        durationMs    int            NULL
    );
GO


 /* ---------------------------------------------------------------------
    Refresh procedure
    --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE reporting.RefreshPageVisits
    @force bit = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @today date = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'GMT Standard Time' AS date);

    -- Cheap check first: nothing to do if already rebuilt today.
    IF @force = 0 AND EXISTS
    (
        SELECT 1 FROM reporting.refreshLog
        WHERE tableName = N'pageVisits' AND refreshedDate >= @today
    )
        RETURN;

    BEGIN TRANSACTION;

    -- Serialise rebuilds: a second caller waits here rather than rebuilding too.
    DECLARE @lockResult int;
    EXEC @lockResult = sp_getapplock
        @Resource    = N'reporting.pageVisits',
        @LockMode    = N'Exclusive',
        @LockOwner   = N'Transaction',
        @LockTimeout = 300000;  -- 5 minutes

    IF @lockResult < 0
        THROW 50001, N'Timed out waiting for the reporting.pageVisits refresh lock.', 1;

    -- Someone else may have finished a rebuild while we waited for the lock.
    IF @force = 0 AND EXISTS
    (
        SELECT 1 FROM reporting.refreshLog
        WHERE tableName = N'pageVisits' AND refreshedDate >= @today
    )
    BEGIN
        COMMIT;
        RETURN;
    END;

    DECLARE @started datetimeoffset = SYSDATETIMEOFFSET();

 /* -------------------------------------------------------------------
    1. Normalise URLs, count visits, split into path segments
    ------------------------------------------------------------------- */
    DROP TABLE IF EXISTS #actionTypes;

    WITH
    distinctRequestedUrls AS
    (
        SELECT
            REGEXP_REPLACE
            (
                REGEXP_REPLACE(ua.requestedUrl, '\?.*$', ''),
                '/(network-switching|wireless-network)(-standard)?(/|$)',
                '/\1-standard\3'
            ) AS requestedUrl,
            COUNT(*) AS visitCount
        FROM
            dbo.userAction ua
        WHERE
            ua.requestedUrl NOT LIKE 'submissionId=%'
        GROUP BY
            ua.requestedUrl
    ),

    usefulUrls AS
    (
        -- Second stage: raw URLs that normalise to the same value are summed.
        SELECT
            dru.requestedUrl,
            SUM(dru.visitCount) AS visitCount
        FROM
            distinctRequestedUrls dru
        WHERE
            dru.requestedUrl NOT IN
            (
                '/accessibility-statement',
                '/apple-touch-icon.png',
                '/apple-touch-icon-precomposed.png',
                '/auth/sign-out',
                '/ConfirmCheckAnswers',
                '/healthy',
                '/home',
                '/prod',
                '/service-unavailable',
                '/service-updates'
            )
        GROUP BY
            dru.requestedUrl
    )

    SELECT
        uu.requestedUrl,
        MAX(uu.visitCount) AS visitCount,
        NULLIF(MAX(CASE WHEN s.ordinal = 1 THEN s.[value] ELSE '' END), '') AS [establishmentType],
        NULLIF(MAX(CASE WHEN s.ordinal = 2 THEN s.[value] ELSE '' END), '') AS [standard],
        NULLIF(MAX(CASE WHEN s.ordinal = 3 THEN s.[value] ELSE '' END), '') AS [topic],
        NULLIF(MAX(CASE WHEN s.ordinal = 4 THEN s.[value] ELSE '' END), '') AS [area],
        NULLIF(MAX(CASE WHEN s.ordinal = 5 THEN s.[value] ELSE '' END), '') AS [page],
        NULLIF(MAX(CASE WHEN s.ordinal = 6 THEN s.[value] ELSE '' END), '') AS [action],
        NULLIF(MAX(CASE WHEN s.ordinal = 7 THEN s.[value] ELSE '' END), '') AS [temp]
    INTO
        #actionTypes
    FROM
        usefulUrls uu
        CROSS APPLY STRING_SPLIT(uu.requestedUrl, '/', 1) s
    GROUP BY
        uu.requestedUrl;

    UPDATE
        #actionTypes
    SET
        [establishmentType] = [standard],
        [standard] = [topic],
        [topic] = [area],
        [area] = [page],
        [page] = [action],
        [action] = [temp],
        [temp] = NULL
    WHERE
        [standard] IN ('school', 'trust');

    UPDATE
        #actionTypes
    SET
        [topic] = NULL
    WHERE
        [topic] = '';

 /* -------------------------------------------------------------------
    2. Join to Contentful and build the report rows
    ------------------------------------------------------------------- */
    DROP TABLE IF EXISTS #pageVisits;

    WITH
    standards AS
    (
        SELECT
            cp.contentfulRef AS pageRef,
            cc.contentfulRef AS standardRef,
            REPLACE(cc.internalName, ' - Category', '') AS standardName
        FROM
            contentful.category cc
            JOIN contentful.[page] cp ON cp.contentfulRef = cc.landingPageRef
        WHERE
            cc.internalName NOT LIKE 'Testing%'
    ),

    topics AS
    (
        SELECT
            cp.contentfulRef AS pageRef,
            cs.contentfulRef AS topicRef,
            ct.[text] AS topicName
        FROM
            contentful.[page] cp
            JOIN contentful.title ct ON ct.contentfulRef = cp.titleRef
            JOIN contentful.section cs ON cs.interstitialPageRef = cp.contentfulRef
    )

    SELECT
        act.requestedUrl AS [url],
        act.visitCount,
        categoryPage.slug AS categorySlug,
        topicPage.slug AS topicSlug,
        CASE
            WHEN std.standardName IS NULL THEN NULL
            ELSE CASE
                WHEN [action] = 'print' THEN 'Print: '
                WHEN [action] = 'print-all' THEN 'Print all: '
                WHEN [action] = 'share' THEN 'Share: '
                WHEN [area] = 'recommendations' AND [action] = 'update-status' THEN 'Update status: '
                ELSE ''
            END +
            COALESCE(cr.header, cq.questionText, tpc.topicName, std.standardName, '') +
            CASE
                WHEN [area] = 'self-assessment' AND [page] = 'continue' THEN ': Continue'
                WHEN [area] = 'self-assessment' AND [page] = 'continue-previous' THEN ': Previous'
                WHEN [area] = 'self-assessment' AND [page] = 'next-question' THEN ': Next'
                WHEN [area] = 'self-assessment' AND [page] = 'restart' THEN ': Restart'
                WHEN [area] = 'check-answers' AND [page] IS NULL THEN ': Check answers'
                WHEN [area] = 'view-answers' AND [page] IS NULL THEN ': View answers'
                ELSE ''
            END
        END AS pageTitle,
        CASE
            WHEN std.standardName IS NULL THEN NULL
            ELSE CASE
                WHEN COALESCE(act.[topic], act.[area], act.[page], act.[action]) IS NULL THEN 'Standard landing page'
                WHEN [area] = 'self-assessment' AND [page] IS NULL THEN 'Topic start page'
                WHEN cq.questionText IS NOT NULL THEN 'Question'
                WHEN [area] = 'self-assessment' AND [page] IN ('continue', 'continue-previous', 'next-question', 'restart') THEN 'Navigation'
                WHEN RIGHT([area], 8) = '-answers' AND [page] IS NULL THEN 'Review answers'
                WHEN [area] = 'self-assessment' AND [page] = 'summary' THEN 'Summary'
                WHEN [area] = 'recommendations' AND [action] IS NULL THEN 'Recommendation'
                WHEN LEFT([action], 5) = 'print' THEN 'Print'
                WHEN [action] = 'share' THEN 'Share'
                WHEN [action] = 'update-status' THEN 'Update status'
                ELSE NULL
            END
        END AS actionType,
        std.standardName,
        std.standardRef,
        tpc.topicName,
        tpc.topicRef
    INTO
        #pageVisits
    FROM
        #actionTypes act
        -- Category / Standard
        LEFT JOIN contentful.[page] categoryPage ON
            categoryPage.slug = [standard]
        LEFT JOIN standards std ON
            std.pageRef = categoryPage.contentfulRef

        -- Section / Topic
        LEFT JOIN contentful.[page] topicPage ON
            topicPage.slug = [topic]
        LEFT JOIN topics tpc ON
            tpc.pageRef = topicPage.contentfulRef

        -- Question
        LEFT JOIN contentful.question cq ON
            cq.slug = act.[page]

        -- Recommendation
        LEFT JOIN contentful.recommendation cr ON
            cr.slug = act.[page]
    WHERE
        categoryPage.internalName NOT LIKE 'Test%';

 /* -------------------------------------------------------------------
    3. Swap into the permanent table
    ------------------------------------------------------------------- */
    -- First run only (do this as an admin): create the table from the
    -- query's own output so the column types always match.
    IF OBJECT_ID(N'reporting.pageVisits') IS NULL
        SELECT * INTO reporting.pageVisits FROM #pageVisits WHERE 1 = 0;

    -- DELETE rather than TRUNCATE: TRUNCATE needs ALTER permission,
    -- which ownership chaining doesn't grant to the Power BI login.
    DELETE FROM reporting.pageVisits;

    INSERT INTO reporting.pageVisits
    SELECT * FROM #pageVisits;

    UPDATE reporting.refreshLog
    SET
        refreshedDate = @today,
        refreshedAt   = SYSDATETIMEOFFSET(),
        durationMs    = DATEDIFF(MILLISECOND, @started, SYSDATETIMEOFFSET())
    WHERE
        tableName = N'pageVisits';

    IF @@ROWCOUNT = 0
        INSERT INTO reporting.refreshLog (tableName, refreshedDate, refreshedAt, durationMs)
        VALUES (N'pageVisits', @today, SYSDATETIMEOFFSET(), DATEDIFF(MILLISECOND, @started, SYSDATETIMEOFFSET()));

    COMMIT;
END;
GO


 /* ---------------------------------------------------------------------
    Read procedure (what Power BI calls)
    --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE reporting.GetPageVisits
AS
BEGIN
    SET NOCOUNT ON;

    EXEC reporting.RefreshPageVisits;  -- no-op if already rebuilt today

    SELECT
        pv.[url],
        pv.visitCount,
        pv.categorySlug,
        pv.topicSlug,
        pv.pageTitle,
        pv.actionType,
        pv.standardName,
        pv.standardRef,
        pv.topicName,
        pv.topicRef
    FROM
        reporting.pageVisits pv
    WHERE
        pv.standardName IS NOT NULL;
END;
GO
