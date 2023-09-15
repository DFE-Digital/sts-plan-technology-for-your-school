CREATE PROCEDURE SubmitAnswer
  @sectionId NVARCHAR(50),
  @sectionName NVARCHAR(50),
  @questionContentfulId NVARCHAR(50),
  @questionText NVARCHAR(MAX),
  @answerContentfulId NVARCHAR(50),
  @answerText NVARCHAR(MAX),
  @userId INT,
  @establishmentId int,
  @maturity NVARCHAR(20)
AS

BEGIN TRY
	BEGIN TRAN

    DECLARE @submissionId INT 
    EXEC SelectOrInsertSubmissionId @sectionId=@sectionId, @sectionName=@sectionName,@establishmentId=@establishmentId, @submissionId = @submissionId OUTPUT;

    DECLARE @answerId INT
    INSERT INTO [dbo].[answer]
      (answerText, contentfulRef)
    VALUES
      (@answerText, @answerContentfulId)
    SELECT @answerId = Scope_Identity() 

    DECLARE @questionId INT
    INSERT INTO [dbo].[question]
      (questionText, contentfulRef)
    VALUES
      (@questionText, @questionContentfulId)
    SELECT @questionId = Scope_Identity() 

    INSERT INTO [dbo].[response]
      (userId, submissionId, questionId, answerId, maturity)
    VALUES
      (@userId, @submissionId, @questionId, @answerId, @maturity)

    DECLARE @responseId INT = Scope_Identity() 

    RETURN @responseId

  COMMIT TRAN
  END TRY
  BEGIN CATCH
    ROLLBACK TRAN
    RETURN 0
  END CATCH