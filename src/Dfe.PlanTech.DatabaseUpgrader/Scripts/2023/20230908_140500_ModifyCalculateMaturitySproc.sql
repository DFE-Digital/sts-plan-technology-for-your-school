
ALTER PROCEDURE [dbo].[calculateMaturityForSubmission] @submissionId int
AS
DECLARE @completed bit = 0

BEGIN TRY
	BEGIN TRAN

        SELECT uniqueMaturity.maturity
        INTO #TempMaturityResponse
        FROM (
                 SELECT
                     response.maturity,
                     response.dateCreated,
                     question.contentfulRef,
                     ROW_NUMBER() OVER (PARTITION BY question.contentfulRef ORDER BY response.dateCreated DESC) AS rowno
                 FROM
                     dbo.response AS response
                 INNER JOIN
                     dbo.question AS question
                 ON
                     response.questionId = question.id
                 WHERE
                     response.submissionId = @submissionId
             ) AS uniqueMaturity
        WHERE
                uniqueMaturity.rowno = 1;

		if (SELECT	count(*) FROM #TempMaturityResponse
			WHERE	maturity = 'Low' COLLATE SQL_Latin1_General_CP1_CI_AS) > 0
		BEGIN
			UPDATE	SUBMISSION
			SET		maturity = 'Low', completed = 1, dateCompleted = GETDATE()
			WHERE	submission.id = @submissionId
			SET		@completed = 1
			COMMIT TRAN
			RETURN	@completed
		END

		if (SELECT	count(*) from #TempMaturityResponse
			WHERE	maturity = 'Medium' COLLATE SQL_Latin1_General_CP1_CI_AS) > 0
		BEGIN
			UPDATE	SUBMISSION
			SET		maturity = 'Medium', completed = 1, dateCompleted = GETDATE()
			WHERE	submission.id = @submissionId
			SET		@completed = 1
			COMMIT TRAN
			RETURN	@completed
		END

		if (SELECT	count(*) FROM #TempMaturityResponse
			WHERE	maturity = 'High' COLLATE SQL_Latin1_General_CP1_CI_AS) > 0
		BEGIN
			UPDATE	SUBMISSION
			SET		maturity = 'High', completed = 1, dateCompleted = GETDATE()
			WHERE	submission.id = @submissionId
			SET		@completed = 1
			COMMIT TRAN
			RETURN	@completed
		END

		--we got down to here that means submission have no value
			UPDATE	SUBMISSION
			SET		maturity = '', completed = 1, dateCompleted = GETDATE()
			WHERE	submission.id = @submissionId
			COMMIT TRAN
END TRY
BEGIN CATCH
	--Error occurred so return 0 and rollback and changes
	ROLLBACK
	RETURN @completed
END CATCH
