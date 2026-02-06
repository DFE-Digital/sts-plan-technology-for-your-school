BEGIN TRAN

	ALTER TABLE [dbo].[establishment]
	ALTER COLUMN [establishmentType] [nvarchar](50);

COMMIT TRAN
