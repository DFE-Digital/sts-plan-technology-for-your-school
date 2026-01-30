SET NOCOUNT ON

DECLARE @id AS BIGINT

EXEC [dbo].[deleteDataForEstablishment]
  @establishmentRef = N'$(ESTABLISHMENT_REF)',
  @establishmentId = @id OUTPUT;

SELECT @id AS EstablishmentId;

PRINT N'Data cleared for establishment ID: ' + COALESCE(CONVERT(nvarchar(50), @id), N'NULL');
