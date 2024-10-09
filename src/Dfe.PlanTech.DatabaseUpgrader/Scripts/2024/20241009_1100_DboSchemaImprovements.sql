-- Remove existing GetCurrentSubmissionId procedure

DROP PROCEDURE GetCurrentSubmissionId

GO

-- Make a new GetCurrentSubmissionId UDF

CREATE FUNCTION GetCurrentSubmissionId(
    @sectionId NVARCHAR(50),
    @establishmentId INT
)
RETURNS INT
AS
BEGIN
    RETURN (
        SELECT TOP 1
            Id
        FROM [dbo].[submission]
        WHERE
              completed = 0
          AND deleted = 0
          AND sectionId = @sectionId
          AND establishmentId = @establishmentId
        ORDER BY dateCreated DESC
    )
END

GO

-- Simplify usage of old GetCurrentSubmissionId proc with the new UDF

ALTER PROCEDURE DeleteCurrentSubmission
    @sectionId NVARCHAR(50),
    @establishmentId INT
AS
BEGIN TRY
    DECLARE @submissionId INT = dbo.GetCurrentSubmissionId (@sectionId, @establishmentId)

    BEGIN TRAN
        UPDATE S
        SET deleted = 1
        FROM [dbo].[submission] S
        WHERE S.id = @submissionId
    COMMIT TRAN

END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH

GO

ALTER PROCEDURE SelectOrInsertSubmissionId
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @establishmentId INT,
    @submissionId INT OUTPUT
AS

BEGIN TRY
    BEGIN TRAN
        SELECT @submissionId = dbo.GetCurrentSubmissionId(@sectionId, @establishmentId)

        IF @submissionId IS NULL
            BEGIN
                INSERT INTO [dbo].[submission]
                    (establishmentId, completed, sectionId, sectionName)
                VALUES
                    (@establishmentId, 0, @sectionId, @sectionName)

                SELECT @submissionId = SCOPE_IDENTITY()
            END
    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH

GO

-- Reduce duplication in SubmitAnswer sproc by reusing question & answer ids where applicable

ALTER PROCEDURE SubmitAnswer
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @questionContentfulId NVARCHAR(50),
    @questionText NVARCHAR(MAX),
    @answerContentfulId NVARCHAR(50),
    @answerText NVARCHAR(MAX),
    @userId INT,
    @establishmentId INT,
    @maturity NVARCHAR(20),
    @responseId INT OUTPUT,
    @submissionId INT OUTPUT
AS

BEGIN TRY
    BEGIN TRAN
        EXEC SelectOrInsertSubmissionId
             @sectionId=@sectionId,
             @sectionName=@sectionName,
             @establishmentId=@establishmentId,
             @submissionId = @submissionId OUTPUT;

        DECLARE @answerId INT
        DECLARE @questionId INT

        SELECT @answerId = Id
        FROM dbo.answer
        WHERE
              answerText = @answerText
          AND contentfulRef = @answerContentfulId

        SELECT @questionId = Id
        FROM dbo.question
        WHERE
              questionText = @questionText
          AND contentfulRef = @questionContentfulId

        IF @answerId IS NULL
            BEGIN
                INSERT INTO [dbo].[answer]
                    (answerText, contentfulRef)
                VALUES
                    (@answerText, @answerContentfulId)
                SELECT @answerId = SCOPE_IDENTITY()
            END

        IF @questionId IS NULL
            BEGIN
                INSERT INTO [dbo].[question]
                    (questionText, contentfulRef)
                VALUES
                    (@questionText, @questionContentfulId)
                SELECT @questionId = SCOPE_IDENTITY()
            END

        INSERT INTO [dbo].[response]
            (userId, submissionId, questionId, answerId, maturity)
        VALUES
            (@userId, @submissionId, @questionId, @answerId, @maturity)

        SELECT @responseId = SCOPE_IDENTITY()
    COMMIT TRAN

END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH

GO

-- Remove existing duplication of questions and answers

SELECT
    MIN(id) questionId,
    questionText,
    contentfulRef
INTO #QuestionKeep
FROM dbo.question
GROUP BY questionText, contentfulRef

SELECT
    MIN(id) answerId,
    answerText,
    contentfulRef
INTO #AnswerKeep
FROM dbo.answer
GROUP BY answerText, contentfulRef

-- Disable trigger so that users don't see "Last completed" get updated to the time that this fix was run
DISABLE TRIGGER tr_response on dbo.response
GO

UPDATE R
SET questionId = QK.questionId,
    answerId   = AK.answerId
FROM dbo.response R
JOIN dbo.answer A ON R.answerId = A.id
JOIN dbo.question Q ON R.questionId = Q.id
JOIN #QuestionKeep QK ON Q.contentfulRef = QK.contentfulRef AND Q.questionText = QK.questionText
JOIN #AnswerKeep AK ON A.contentfulRef = AK.contentfulRef AND A.answerText = AK.answerText
GO

ENABLE TRIGGER tr_response on dbo.response
GO

-- deleteDataForEstablishment doesn't remove the questions and answers tied to the responses that get deleted
-- it now doesn't need to because the ids are shared between responses, but we can clean out all the old ones

DELETE Q
FROM dbo.question Q
LEFT JOIN dbo.response R ON Q.id = R.questionId
WHERE
    R.id IS NULL

DELETE A
FROM dbo.answer A
LEFT JOIN dbo.response R ON A.id = R.answerId
WHERE
    R.id IS NULL

DROP TABLE #QuestionKeep
DROP TABLE #AnswerKeep

GO
-- Add index on for dbo.response submissionId

CREATE INDEX IX_response_submissionId on dbo.response (submissionId)

GO
