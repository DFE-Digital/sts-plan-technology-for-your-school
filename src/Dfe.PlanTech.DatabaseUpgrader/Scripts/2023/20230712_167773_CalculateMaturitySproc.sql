CREATE PROCEDURE [dbo].[calculateMaturityForSubmission] @submissionId int
AS
DECLARE @completed bit = 0
DECLARE @low INT
DECLARE @mid int
DECLARE @high int

BEGIN TRY
	BEGIN TRAN

		SELECT	maturity
		INTO	#TempMaturityResponse
		FROM	response rp
		WHERE	rp.submissionId = @submissionId

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
			WHERE	maturity = 'Mid' COLLATE SQL_Latin1_General_CP1_CI_AS) > 0
		BEGIN
			UPDATE	SUBMISSION
			SET		maturity = 'Mid', completed = 1, dateCompleted = GETDATE()
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
	ROLLBACK TRAN
	RETURN @completed
END CATCH
