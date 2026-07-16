IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_userAction_userId'
      AND object_id = OBJECT_ID(N'dbo.userAction')
)
BEGIN
    CREATE INDEX IX_userAction_userId
        ON dbo.userAction(userId);
END;
GO