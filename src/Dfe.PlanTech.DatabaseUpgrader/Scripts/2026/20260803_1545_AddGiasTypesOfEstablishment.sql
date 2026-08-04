CREATE TABLE gias.typeOfEstablishment (
    typeOfEstablishmentCode  INT            NOT NULL,
    typeOfEstablishmentName  NVARCHAR(100)  NOT NULL,
    CONSTRAINT PK_giasTypeOfEstablishment PRIMARY KEY (typeOfEstablishmentCode)
);
GO

ALTER TABLE gias.establishment
ADD [typeOfEstablishmentCode] INT NULL;
GO

ALTER TABLE gias.establishment
ADD CONSTRAINT FK_giasEstablishment_typeOfEstablishment
    FOREIGN KEY (typeOfEstablishmentCode) REFERENCES gias.typeOfEstablishment (typeOfEstablishmentCode);
GO
