BEGIN TRAN
  SELECT Id into #submissionIds FROM [dbo].[submission] WHERE sectionId IS NULL OR sectionName IS NULL

  DELETE FROM [dbo].[response] WHERE EXISTS (SELECT *
                              FROM #submissionIds
                              where submissionId = #submissionIds.id)

  DELETE FROM [dbo].[submission] WHERE sectionId IS NULL OR sectionName IS NULL

  ALTER TABLE [dbo].[submission] ALTER COLUMN [sectionId] [NVARCHAR](50) NOT NULL
  ALTER TABLE [dbo].[submission] ALTER COLUMN [sectionName] [NVARCHAR](50) NOT NULL
  ALTER TABLE [dbo].[submission] DROP COLUMN [recomendationId]

COMMIT TRAN
