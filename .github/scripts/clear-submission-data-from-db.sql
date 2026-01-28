SET NOCOUNT ON

DECLARE @id AS BIGINT

EXEC [dbo].[testWorkflow]
  @establishmentRef = N'$ESTABLISHMENT_REF',
  @establishmentId = @id OUTPUT;

SELECT @id AS EstablishmentId;
