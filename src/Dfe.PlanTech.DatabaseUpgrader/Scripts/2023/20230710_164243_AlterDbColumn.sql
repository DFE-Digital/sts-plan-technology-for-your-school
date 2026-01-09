BEGIN TRAN

	ALTER TABLE [dbo].[submission]
	ALTER COLUMN [sectionId] [nvarchar](50);

COMMIT TRAN
