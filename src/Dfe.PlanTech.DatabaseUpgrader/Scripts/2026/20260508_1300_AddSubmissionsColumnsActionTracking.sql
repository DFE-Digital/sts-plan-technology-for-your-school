ALTER TABLE dbo.submission
ADD createdUserActionId UNIQUEIDENTIFIER NULL,
    lastUpdatedUserActionId UNIQUEIDENTIFIER NULL,
    completedUserActionId UNIQUEIDENTIFIER NULL;
GO