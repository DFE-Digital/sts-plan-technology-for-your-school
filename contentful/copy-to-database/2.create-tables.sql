-- =========================
-- Create Contentful tables
-- =========================

IF OBJECT_ID('contentful.answers', 'U') IS NOT NULL DROP TABLE contentful.answers;
IF OBJECT_ID('contentful.categories', 'U') IS NOT NULL DROP TABLE contentful.categories;
IF OBJECT_ID('contentful.categorySections', 'U') IS NOT NULL DROP TABLE contentful.categorySections;
IF OBJECT_ID('contentful.questionAnswers', 'U') IS NOT NULL DROP TABLE contentful.questionAnswers;
IF OBJECT_ID('contentful.questions', 'U') IS NOT NULL DROP TABLE contentful.questions;
IF OBJECT_ID('contentful.recommendationAnswerStatuses', 'U') IS NOT NULL DROP TABLE contentful.recommendationAnswerStatuses;
IF OBJECT_ID('contentful.recommendationChunks', 'U') IS NOT NULL DROP TABLE contentful.recommendationChunks;
IF OBJECT_ID('contentful.recommendations', 'U') IS NOT NULL DROP TABLE contentful.recommendations;
IF OBJECT_ID('contentful.sectionQuestions', 'U') IS NOT NULL DROP TABLE contentful.sectionQuestions;
IF OBJECT_ID('contentful.sectionRecommendations', 'U') IS NOT NULL DROP TABLE contentful.sectionRecommendations;
IF OBJECT_ID('contentful.sections', 'U') IS NOT NULL DROP TABLE contentful.sections;

IF OBJECT_ID('contentful.category', 'U') IS NOT NULL DROP TABLE contentful.category;
CREATE TABLE contentful.category (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    landingPageRef NVARCHAR(32),
    sectionRefs NVARCHAR(MAX),
);

IF OBJECT_ID('contentful.categorySection', 'U') IS NOT NULL DROP TABLE contentful.categorySection;
CREATE TABLE contentful.categorySection (
    categoryRef NVARCHAR(32),
    sectionRef NVARCHAR(32)
);

IF OBJECT_ID('contentful.page', 'U') IS NOT NULL DROP TABLE contentful.[page];
CREATE TABLE contentful.[page] (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    slug NVARCHAR(255),
    titleRef NVARCHAR(32),
);

IF OBJECT_ID('contentful.title', 'U') IS NOT NULL DROP TABLE contentful.title;
CREATE TABLE contentful.title (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    [text] NVARCHAR(MAX),
);

IF OBJECT_ID('contentful.section', 'U') IS NOT NULL DROP TABLE contentful.section;
CREATE TABLE contentful.section (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    [name] NVARCHAR(MAX),
    shortDescription NVARCHAR(MAX),
    interstitialPageRef NVARCHAR(32),
    questionRefs NVARCHAR(MAX),
    recommendationRefs NVARCHAR(MAX)
);

IF OBJECT_ID('contentful.sectionQuestion', 'U') IS NOT NULL DROP TABLE contentful.sectionQuestion;
CREATE TABLE contentful.sectionQuestion (
    sectionRef NVARCHAR(32),
    questionRef NVARCHAR(32)
);

IF OBJECT_ID('contentful.sectionRecommendation', 'U') IS NOT NULL DROP TABLE contentful.sectionRecommendation;
CREATE TABLE contentful.sectionRecommendation (
    sectionRef NVARCHAR(32),
    recommendationRef NVARCHAR(32)
);

IF OBJECT_ID('contentful.question') IS NOT NULL DROP TABLE contentful.question;
CREATE TABLE contentful.question (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    questionText NVARCHAR(MAX),
    helpText NVARCHAR(MAX),
    slug NVARCHAR(255),
    answerRefs NVARCHAR(MAX)
);

IF OBJECT_ID('contentful.questionAnswer') IS NOT NULL DROP TABLE contentful.questionAnswer;
CREATE TABLE contentful.questionAnswer (
    questionRef NVARCHAR(32),
    answerRef NVARCHAR(32)
);

IF OBJECT_ID('contentful.answer') IS NOT NULL DROP TABLE contentful.answer;
CREATE TABLE contentful.answer (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    answerText NVARCHAR(MAX),
    nextQuestionRef NVARCHAR(MAX)
);

IF OBJECT_ID('contentful.recommendation') IS NOT NULL DROP TABLE contentful.recommendation;
CREATE TABLE contentful.recommendation (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    [header] NVARCHAR(500),
    slug NVARCHAR(255),
    textBodyRef NVARCHAR(32),
	questioncontentfulRef NVARCHAR(32),
    completingAnswerRefs NVARCHAR(MAX),
    inProgressAnswerRefs NVARCHAR(MAX)
);

IF OBJECT_ID('contentful.recommendationAnswerStatus') IS NOT NULL DROP TABLE contentful.recommendationAnswerStatus;
CREATE TABLE contentful.recommendationAnswerStatus (
    recommendationRef NVARCHAR(32),
    answerRef NVARCHAR(32),
    recommendationStatus NVARCHAR(32)
);

IF OBJECT_ID('contentful.textBody') IS NOT NULL DROP TABLE contentful.textBody;
CREATE TABLE contentful.textBody (
    contentfulRef NVARCHAR(32),
    internalName NVARCHAR(255),
    richText NVARCHAR(MAX)
);
