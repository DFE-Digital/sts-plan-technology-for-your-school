IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [name] = 'groupReadActivity')
BEGIN
    CREATE TABLE dbo.groupReadActivity
    (
        id INT IDENTITY (1,1) NOT NULL,
        userId INT NOT NULL,
        establishmentId INT NOT NULL,
        selectedEstablishmentId INT NOT NULL,
        selectedEstablishmentName NVARCHAR(50) NOT NULL,
        dateSelected DATETIME NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT PK_groupReadActivity PRIMARY KEY CLUSTERED (id ASC)
    );
END;
