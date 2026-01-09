BEGIN TRAN

	ALTER TABLE [dbo].[establishment]
	ALTER COLUMN [orgName] [nvarchar](200) NOT NULL;

COMMIT TRAN
