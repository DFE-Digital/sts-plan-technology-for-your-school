ALTER TABLE dbo.response
ADD userEstablishmentId INT NULL
GO

-- FK
ALTER TABLE dbo.response
    ADD CONSTRAINT FK_response_userEstablishment
        FOREIGN KEY (userEstablishmentId) REFERENCES dbo.[establishment](id);
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Reduce duplication in SubmitAnswer sproc by reusing question & answer IDs where applicable

ALTER PROCEDURE [dbo].[SubmitAnswer]
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @questionContentfulId NVARCHAR(50),
    @questionText NVARCHAR(MAX),
    @answerContentfulId NVARCHAR(50),
    @answerText NVARCHAR(MAX),
    @userId INT,
    @userEstablishmentId INT,
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
            (userId, userEstablishmentId, submissionId, questionId, answerId, maturity)
        VALUES
            (@userId, @userEstablishmentId, @submissionId, @questionId, @answerId, @maturity)

        SELECT @responseId = SCOPE_IDENTITY()
    COMMIT TRAN

END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH
