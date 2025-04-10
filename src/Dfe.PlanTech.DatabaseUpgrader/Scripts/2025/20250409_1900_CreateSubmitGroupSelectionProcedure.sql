CREATE PROCEDURE SubmitGroupSelection
  @userId INT,
  @establishmentId INT,
  @selectedEstablishmentId INT,
  @selectedEstablishmentName NVARCHAR(50),
  @selectionId INT OUTPUT
AS

BEGIN TRY
	BEGIN TRAN

    INSERT INTO [dbo].[groupReadActivity]
      (userId, establishmentId, selectedEstablishmentId, selectedEstablishmentName)
    VALUES
      (@userId, @establishmentId, @selectedEstablishmentId, @selectedEstablishmentName)

    SELECT @selectionId = Scope_Identity() 
  COMMIT TRAN

  END TRY
  BEGIN CATCH
    ROLLBACK TRAN
  END CATCH
