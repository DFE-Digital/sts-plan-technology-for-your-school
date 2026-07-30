DECLARE @json NVARCHAR(MAX);

-- =========================
-- Parse Categories
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'category'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.category (contentfulRef, internalName, landingPageRef, sectionRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'landingPage' THEN JSON_VALUE(f.value, '$.sys.id') END) AS landingPageRef,
    STRING_AGG(CAST(JSON_VALUE(s.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') AS sectionRefs
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
OUTER APPLY OPENJSON(i.[value], '$.fields.sections') s
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

INSERT INTO contentful.categorySection (categoryRef, sectionRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS categoryRef,
    JSON_VALUE(s.[value], '$.sys.id') AS sectionRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.sections') s;

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Pages
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'page'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.[page] (contentfulRef, internalName, slug, titleRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'slug' THEN f.value END) AS slug,
    MAX(CASE WHEN f.[key] = 'title' THEN JSON_VALUE(f.value, '$.sys.id') END) AS titleRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Titles
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'title'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.title (contentfulRef, internalName, [text])
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'text' THEN f.value END) AS [text]
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Sections
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'section'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.section (contentfulRef, internalName, [name], shortDescription, interstitialPageRef, questionRefs, recommendationRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'name' THEN f.value END) AS [name],
    MAX(CASE WHEN f.[key] = 'shortDescription' THEN f.value END) AS shortDescription,
    MAX(CASE WHEN f.[key] = 'interstitialPage' THEN JSON_VALUE(f.value, '$.sys.id') END) AS interstitialPageRef,
    STRING_AGG(CAST(JSON_VALUE(q.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') AS questionRefs,
    STRING_AGG(CAST(JSON_VALUE(cr.[value], '$.sys.id') AS NVARCHAR(MAX)), ',') AS recommendationRefs
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
OUTER APPLY OPENJSON(i.[value], '$.fields.questions') q
OUTER APPLY OPENJSON(i.[value], '$.fields.coreRecommendations') cr
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

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

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Questions
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'question'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.question (contentfulRef, internalName, questionText, helpText, slug, answerRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'text' THEN f.value END) AS questionText,
    MAX(CASE WHEN f.[key] = 'helpText' THEN f.value END) AS helpText,
    MAX(CASE WHEN f.[key] = 'slug' THEN f.value END) AS slug,
    STRING_AGG(JSON_VALUE(ans.[value], '$.sys.id'), ',') AS answerRefs
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
OUTER APPLY OPENJSON(i.[value], '$.fields.answers') ans
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

INSERT INTO contentful.questionAnswer (questionRef, answerRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS questionRef,
    JSON_VALUE(ans.[value], '$.sys.id') AS answerRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.answers') ans;

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Answers
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'answer'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.answer (contentfulRef, internalName, answerText, nextQuestionRef)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'text' THEN f.value END) AS answerText,
    MAX(CASE WHEN f.[key] = 'nextQuestion' THEN JSON_VALUE(f.value, '$.sys.id') END) AS nextQuestionRef
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Recommendations
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'recommendationChunk'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.recommendation (contentfulRef, internalName, [header], slug, textBodyRef, questionContentfulRef, completingAnswerRefs, inProgressAnswerRefs)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'header' THEN f.value END) AS [header],
    MAX(CASE WHEN f.[key] = 'slug' THEN f.value END) AS slug,
    MAX(CASE WHEN f.[key] = 'content' THEN JSON_VALUE([content].[value], '$.sys.id') END) AS textBodyRef,
	MAX(CASE WHEN f.[key] = 'question' THEN JSON_VALUE(f.value, '$.sys.id') END) AS questionContentfulRef,
    STRING_AGG(JSON_VALUE(completingAnswers.[value], '$.sys.id'), ',') AS completingAnswerRefs,
    STRING_AGG(JSON_VALUE(inProgressAnswers.[value], '$.sys.id'), ',') AS inProgressAnswerRefs
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
OUTER APPLY OPENJSON(i.[value], '$.fields.content') [content]
OUTER APPLY OPENJSON(i.[value], '$.fields.completingAnswers') completingAnswers
OUTER APPLY OPENJSON(i.[value], '$.fields.inProgressAnswers') inProgressAnswers
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

INSERT INTO contentful.recommendationAnswerStatus (recommendationRef, answerRef, recommendationStatus)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS recommendationRef,
    JSON_VALUE(completingAnswers.[value], '$.sys.id') AS answerRef,
	'Complete' AS recommendationStatus
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.completingAnswers') completingAnswers;

INSERT INTO contentful.recommendationAnswerStatus (recommendationRef, answerRef, recommendationStatus)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS recommendationRef,
    JSON_VALUE(inProgressAnswers.[value], '$.sys.id') AS answerRef,
	'InProgress' AS recommendationStatus
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields.inProgressAnswers') inProgressAnswers;

SET ANSI_WARNINGS ON;

-- =========================
-- Parse Text Bodies
-- =========================
SELECT TOP 1
    @json = CAST(JsonResponse COLLATE Latin1_General_100_CI_AS_SC_UTF8 AS NVARCHAR(MAX))
FROM contentful.contentfulImport
WHERE ContentType = 'textBody'
ORDER BY ImportedAt DESC;

SET ANSI_WARNINGS OFF;

INSERT INTO contentful.textBody (contentfulRef, internalName, richText)
SELECT
    JSON_VALUE(i.[value], '$.sys.id') AS contentfulRef,
    MAX(CASE WHEN f.[key] = 'internalName' THEN f.value END) AS internalName,
    MAX(CASE WHEN f.[key] = 'richText' THEN f.value END) AS richText
FROM OPENJSON(@json, '$.items') i
CROSS APPLY OPENJSON(i.[value], '$.fields') f
GROUP BY JSON_VALUE(i.[value], '$.sys.id');

SET ANSI_WARNINGS ON;

-- =========================
-- Clean data
-- =========================

SET ANSI_WARNINGS OFF;

UPDATE
    contentful.question
SET
    internalName = REPLACE(internalName, N'â€™', N'’'),
    questionText = REPLACE(questionText, N'â€™', N'’'),
    helpText = REPLACE(helpText, N'â€™', N'’')

UPDATE
    contentful.answer
SET
    internalName = REPLACE(internalName, N'â€™', N'’'),
    answerText = REPLACE(answerText, N'â€™', N'’')

UPDATE
    contentful.recommendation
SET
    internalName = REPLACE(internalName, N'â€™', N'’'),
    header = REPLACE(header, N'â€™', N'’')

UPDATE
    contentful.textBody
SET
    internalName = REPLACE(internalName, N'â€™', N'’'),
    richText = REPLACE(REPLACE(richText, N'â€™', N'’'), N'Â', N'')

SET ANSI_WARNINGS ON;
