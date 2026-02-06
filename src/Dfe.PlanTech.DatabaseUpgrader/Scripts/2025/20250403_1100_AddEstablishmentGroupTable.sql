CREATE TABLE dbo.establishmentGroup(
    id INT IDENTITY(1, 1) CONSTRAINT PK_establishmentGroup PRIMARY KEY,
    uid NVARCHAR(50) UNIQUE,
    groupName NVARCHAR(200),
    groupType NVARCHAR(200),
    groupStatus NVARCHAR(200),
)
GO

CREATE TABLE dbo.establishmentLink(
    id INT IDENTITY(1, 1) CONSTRAINT PK_establishmentLink PRIMARY KEY,
    groupUid NVARCHAR(50) CONSTRAINT FK_groupUid_establishmentGroup REFERENCES dbo.establishmentGroup(uid),
    establishmentName NVARCHAR(200),
    urn VARCHAR(32)
)
GO

ALTER TABLE dbo.establishment DISABLE TRIGGER tr_establishment

GO

-- No FK, as there is currently no process to remove old records from this table (if establishments get removed)
ALTER TABLE dbo.establishment ADD groupUid NVARCHAR(50)

GO

ALTER TABLE dbo.establishment ENABLE TRIGGER tr_establishment

GO

CREATE OR ALTER PROCEDURE dbo.UpdateEstablishmentData AS
BEGIN TRY
    BEGIN TRAN

        -- Disable FK for the duration of the update
        ALTER TABLE dbo.establishmentLink
            NOCHECK CONSTRAINT FK_groupUid_establishmentGroup;

        -- Update establishment Group records
        MERGE INTO dbo.establishmentGroup AS target
        USING #EstablishmentGroup AS source
        ON target.uid = source.uid
        WHEN MATCHED THEN
            UPDATE SET
               target.groupName = source.groupName,
               target.groupType = source.groupType,
               target.groupStatus = source.groupStatus
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (uid, groupName, groupType, groupStatus)
            VALUES (source.uid, source.groupName, source.groupType, source.groupStatus)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        -- Update establishment Link records
        MERGE INTO dbo.establishmentLink AS target
        USING #EstablishmentLink AS source
        ON target.urn = source.urn and target.groupUid = source.groupUid
        WHEN MATCHED THEN
            UPDATE SET
               target.establishmentName = source.establishmentName
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (urn, groupUid, establishmentName)
            VALUES (source.urn, source.groupUid, source.establishmentName)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        -- Re-enable the FK with check
        ALTER TABLE dbo.establishmentLink
            WITH CHECK
                CHECK CONSTRAINT FK_groupUid_establishmentGroup;

    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH

GO
