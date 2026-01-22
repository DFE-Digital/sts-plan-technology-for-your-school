BEGIN TRAN

	ALTER TABLE [dbo].[user]
	ALTER COLUMN [dfeSignInRef] [nvarchar](40) NOT NULL;

COMMIT TRAN
