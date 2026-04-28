CREATE TABLE dbo.userAction
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_userAction PRIMARY KEY
        CONSTRAINT DF_userAction_Id DEFAULT NEWID(),

    UserId INT NOT NULL,
    EstablishmentId INT NULL,
    MatEstablishmentId INT NULL,

    RequestedUrl NVARCHAR(2048) NOT NULL,

    DateCreated DATETIME2(7) NOT NULL
        CONSTRAINT DF_userAction_DateCreated DEFAULT SYSUTCDATETIME()
);
GO