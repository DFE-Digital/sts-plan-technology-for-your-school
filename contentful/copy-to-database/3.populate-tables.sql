-- =========================
-- Load the latest import for each content type
-- =========================
DECLARE @imports TABLE (contentType NVARCHAR(100) PRIMARY KEY, json NVARCHAR(MAX));

INSERT @imports (contentType, json)
SELECT ContentType, CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM (
    SELECT ContentType, JsonResponse,
           ROW_NUMBER() OVER (PARTITION BY ContentType ORDER BY ImportedAt DESC) AS rn
    FROM contentful.contentfulImport
) latest
WHERE rn = 1;

-- =========================
-- Clean data (once, on the raw JSON, so every table and column gets it)
-- UTF-8 that was decoded as Windows-1252: "â€x" and "Âx" sequences
-- =========================
DECLARE @a    NVARCHAR(1) = NCHAR(226);   -- â   (NVARCHAR, not NCHAR: NCHAR pads the VALUES column below with spaces)
DECLARE @euro NVARCHAR(1) = NCHAR(8364);  -- €

DECLARE @fixes TABLE (seq INT PRIMARY KEY, bad NVARCHAR(10) NOT NULL, good NVARCHAR(10) NOT NULL);

INSERT @fixes (seq, bad, good) VALUES
    ( 1, @a + @euro + NCHAR(8482), NCHAR(8217)),  -- â€™ → ’  right single quote / apostrophe
    ( 2, @a + @euro + NCHAR(732),  NCHAR(8216)),  -- â€˜ → ‘  left single quote
    ( 3, @a + @euro + NCHAR(339),  NCHAR(8220)),  -- â€œ → “  left double quote
    ( 4, @a + @euro + NCHAR(157),  NCHAR(8221)),  -- â€  → ”  right double quote (0x9D has no 1252 char)
    ( 5, @a + @euro + NCHAR(8220), NCHAR(8211)),  -- â€“ → –  en dash
    ( 6, @a + @euro + NCHAR(8221), NCHAR(8212)),  -- â€” → —  em dash
    ( 7, @a + @euro + NCHAR(8216), NCHAR(8209)),  -- â€‘ → ‑  non-breaking hyphen
    ( 8, @a + @euro + NCHAR(166),  NCHAR(8230)),  -- â€¦ → …  ellipsis
    ( 9, @a + @euro + NCHAR(162),  NCHAR(8226)),  -- â€¢ → •  bullet
    (10, @a + @euro + NCHAR(8249), N''),          -- â€‹ → (zero-width space, removed)
    (11, @a + @euro + NCHAR(168),  NCHAR(8232)),  -- â€¨ → U+2028 line separator (Word soft break)
    (12, @a + @euro + NCHAR(175),  NCHAR(8239)),  -- â€¯ → U+202F narrow no-break space
    (40, NCHAR(194),               N'');          -- Â   → removed (Â  → nbsp, Â£ → £, Â° → °); case-sensitive, so â is untouched

DECLARE @seq INT = 0, @bad NVARCHAR(10), @good NVARCHAR(10);

WHILE 1 = 1
BEGIN
    SELECT TOP 1 @seq = seq, @bad = bad, @good = good
    FROM @fixes
    WHERE seq > @seq
    ORDER BY seq;

    IF @@ROWCOUNT = 0 BREAK;

    UPDATE @imports
    SET json = REPLACE(json COLLATE Latin1_General_100_BIN2, @bad, @good)
    WHERE CHARINDEX(@bad, json COLLATE Latin1_General_100_BIN2) > 0;
END;

/*
  Parsing notes:
  - Text fields use a scalar subquery per field rather than MAX(CASE ...) + GROUP BY, so there are no
    "Null value is eliminated" warnings and no need for ANSI_WARNINGS OFF (which silently truncates).
  - Ref lists use their own subquery, so they aren't multiplied by the number of fields / other arrays.
  - SET @json = (SELECT ...) gives NULL if a content type is missing, instead of keeping the previous one.
*/
DECLARE @json NVARCHAR(MAX);

-- =========================
-- Parse Categories
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'category');

INSERT INTO contentful.category (contentfulRef, internalName, landingPageRef, sectionRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    JSON_VALUE(i.[value], '$.fields.landingPage.sys.id') AS landingPageRef,
    (SELECT STRING_AGG(CAST(JSON_VALUE(s.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') WITHIN GROUP (ORDER BY CAST(s.[key] AS INT))
     FROM OPENJSON(i.[value], '$.fields.sections') s) AS sectionRefs
FROM OPENJSON(@json, '$.items') i;

INSERT INTO contentful.categorySection (categoryRef, sectionRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS categoryRef,
    JSON_VALUE(s.[value], '$.sys.id') AS sectionRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.sections') s;

-- =========================
-- Parse Pages
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'page');

INSERT INTO contentful.[page] (contentfulRef, internalName, slug, titleRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'slug') AS slug,
    JSON_VALUE(i.[value], '$.fields.title.sys.id') AS titleRef
FROM OPENJSON(@json, '$.items') i;

-- =========================
-- Parse Titles
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'title');

INSERT INTO contentful.title (contentfulRef, internalName, [text])
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'text') AS [text]
FROM OPENJSON(@json, '$.items') i;

-- =========================
-- Parse Sections
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'section');

INSERT INTO contentful.section (contentfulRef, internalName, [name], shortDescription, interstitialPageRef, questionRefs, recommendationRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'name') AS [name],
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'shortDescription') AS shortDescription,
    JSON_VALUE(i.[value], '$.fields.interstitialPage.sys.id') AS interstitialPageRef,
    (SELECT STRING_AGG(CAST(JSON_VALUE(q.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') WITHIN GROUP (ORDER BY CAST(q.[key] AS INT))
     FROM OPENJSON(i.[value], '$.fields.questions') q) AS questionRefs,
    (SELECT STRING_AGG(CAST(JSON_VALUE(cr.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') WITHIN GROUP (ORDER BY CAST(cr.[key] AS INT))
     FROM OPENJSON(i.[value], '$.fields.coreRecommendations') cr) AS recommendationRefs
FROM OPENJSON(@json, '$.items') i;

INSERT INTO contentful.sectionQuestion (sectionRef, questionRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS sectionRef,
    JSON_VALUE(q.[value], '$.sys.id') AS questionRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.questions') q;

INSERT INTO contentful.sectionRecommendation (sectionRef, recommendationRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS sectionRef,
    JSON_VALUE(cr.[value], '$.sys.id') AS recommendationRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.coreRecommendations') cr;

-- =========================
-- Parse Questions
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'question');

INSERT INTO contentful.question (contentfulRef, internalName, questionText, helpText, slug, answerRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'text') AS questionText,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'helpText') AS helpText,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'slug') AS slug,
    (SELECT STRING_AGG(CAST(JSON_VALUE(ans.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') WITHIN GROUP (ORDER BY CAST(ans.[key] AS INT))
     FROM OPENJSON(i.[value], '$.fields.answers') ans) AS answerRefs
FROM OPENJSON(@json, '$.items') i;

INSERT INTO contentful.questionAnswer (questionRef, answerRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS questionRef,
    JSON_VALUE(ans.[value], '$.sys.id') AS answerRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.answers') ans;

-- =========================
-- Parse Answers
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'answer');

INSERT INTO contentful.answer (contentfulRef, internalName, answerText, nextQuestionRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'text') AS answerText,
    JSON_VALUE(i.[value], '$.fields.nextQuestion.sys.id') AS nextQuestionRef
FROM OPENJSON(@json, '$.items') i;

-- =========================
-- Parse Recommendations
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'recommendationChunk');

INSERT INTO contentful.recommendation (contentfulRef, internalName, [header], slug, textBodyRef, questionContentfulRef, completingAnswerRefs, inProgressAnswerRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'header') AS [header],
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'slug') AS slug,
    JSON_VALUE(i.[value], '$.fields.content[0].sys.id') AS textBodyRef,   -- first content item
    JSON_VALUE(i.[value], '$.fields.question.sys.id') AS questionContentfulRef,
    (SELECT STRING_AGG(CAST(JSON_VALUE(ca.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') WITHIN GROUP (ORDER BY CAST(ca.[key] AS INT))
     FROM OPENJSON(i.[value], '$.fields.completingAnswers') ca) AS completingAnswerRefs,
    (SELECT STRING_AGG(CAST(JSON_VALUE(ipa.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') WITHIN GROUP (ORDER BY CAST(ipa.[key] AS INT))
     FROM OPENJSON(i.[value], '$.fields.inProgressAnswers') ipa) AS inProgressAnswerRefs
FROM OPENJSON(@json, '$.items') i;

INSERT INTO contentful.recommendationAnswerStatus (recommendationRef, answerRef, recommendationStatus)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS recommendationRef,
    JSON_VALUE(ca.[value], '$.sys.id') AS answerRef,
    'Complete' AS recommendationStatus
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.completingAnswers') ca;

INSERT INTO contentful.recommendationAnswerStatus (recommendationRef, answerRef, recommendationStatus)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS recommendationRef,
    JSON_VALUE(ipa.[value], '$.sys.id') AS answerRef,
    'InProgress' AS recommendationStatus
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.inProgressAnswers') ipa;

-- =========================
-- Parse Text Bodies
-- =========================
SET @json = (SELECT json FROM @imports WHERE contentType = N'textBody');

INSERT INTO contentful.textBody (contentfulRef, internalName, richText)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'internalName') AS internalName,
    (SELECT [value] FROM OPENJSON(i.[value], '$.fields') WHERE [key] = N'richText') AS richText
FROM OPENJSON(@json, '$.items') i;

-- =========================
-- Optional check: anything left that still looks like mojibake, across every import
-- (€ = leftover "â€x", Â / Ã = two-byte sequences, U+FFFD = characters lost before they reached SQL)
-- =========================
SELECT
    im.contentType,
    ch.label,
    SUBSTRING(im.json, CHARINDEX(ch.c, im.json COLLATE Latin1_General_100_BIN2) - 40, 80) AS firstExample
FROM @imports im
CROSS JOIN (VALUES (NCHAR(8364), N'€'), (NCHAR(194), N'Â'), (NCHAR(195), N'Ã'), (NCHAR(65533), N'U+FFFD')) ch (c, label)
WHERE CHARINDEX(ch.c, im.json COLLATE Latin1_General_100_BIN2) > 0;
