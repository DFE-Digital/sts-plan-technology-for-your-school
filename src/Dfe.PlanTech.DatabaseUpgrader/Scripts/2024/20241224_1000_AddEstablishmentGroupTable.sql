CREATE TABLE dbo.establishmentGroup(
    id INT IDENTITY(1, 1) CONSTRAINT PK_establishmentGroup PRIMARY KEY,
    uid NVARCHAR(50) UNIQUE,
    groupName NVARCHAR(500),
    groupType NVARCHAR(200),
    groupTypeCode NVARCHAR(50)
)
GO

ALTER TABLE dbo.establishment DISABLE TRIGGER tr_establishment
GO

ALTER TABLE dbo.establishment
    ADD groupUid NVARCHAR(50)
        CONSTRAINT FK_groupUid_establishmentGroup REFERENCES dbo.establishmentGroup(uid)
GO

ALTER TABLE dbo.establishment ENABLE TRIGGER tr_establishment
GO

CREATE OR ALTER PROCEDURE dbo.UpdateEstablishmentData AS
BEGIN TRY
    BEGIN TRAN
        -- Disable FK for the duration of the update
        ALTER TABLE dbo.establishment
            NOCHECK CONSTRAINT FK_groupUid_establishmentGroup;

        -- Update establishment Group records
        MERGE INTO dbo.establishmentGroup AS target
        USING #EstablishmentGroup AS source
        ON target.uid = source.uid
        WHEN MATCHED THEN
            UPDATE SET
               target.groupName = source.groupName,
               target.groupType = source.groupType,
               target.groupTypeCode = source.groupTypeCode
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (uid, groupName, groupType, groupTypeCode)
            VALUES (source.uid, source.groupName, source.groupType, source.groupTypeCode)
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE;

        -- Update establishment links
        UPDATE E
        SET E.groupUid = EL.uid
        FROM dbo.establishment E
        LEFT JOIN #EstablishmentLink EL on E.orgName = EL.establishmentName
        WHERE
            COALESCE(E.groupUid, '') <> COALESCE(EL.uid, '') -- avoid non-updates

        -- Re-enable the FK with check
        ALTER TABLE dbo.establishment
            WITH CHECK
                CHECK CONSTRAINT FK_groupUid_establishmentGroup;

    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH

GO
