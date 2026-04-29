CREATE TABLE dbo.userAction
(
    id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_userAction PRIMARY KEY,

    userId INT NOT NULL,
    establishmentId INT NULL,
    matEstablishmentId INT NULL,

    requestedUrl NVARCHAR(2048) NOT NULL,

    dateCreated DATETIME2(7) NOT NULL
        CONSTRAINT DF_userAction_dateCreated DEFAULT SYSUTCDATETIME()
);
GO