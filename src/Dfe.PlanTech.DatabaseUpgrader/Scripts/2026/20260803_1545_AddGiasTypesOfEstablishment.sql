IF OBJECT_ID(N'gias.typeOfEstablishment', N'U') IS NULL
BEGIN
    CREATE TABLE gias.typeOfEstablishment (
        typeOfEstablishmentCode  INT            NOT NULL,
        typeOfEstablishmentName  NVARCHAR(100)  NOT NULL,
        CONSTRAINT PK_giasTypeOfEstablishment PRIMARY KEY (typeOfEstablishmentCode)
    );
END;
GO

IF COL_LENGTH(N'gias.establishment', N'typeOfEstablishmentCode') IS NULL
BEGIN
    ALTER TABLE gias.establishment
    ADD [typeOfEstablishmentCode] INT NULL;
END
ELSE IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID(N'gias.establishment')
      AND c.name = N'typeOfEstablishmentCode'
      AND (t.name <> N'int' OR c.is_nullable <> 1)
)
BEGIN
    ALTER TABLE gias.establishment
    ALTER COLUMN [typeOfEstablishmentCode] INT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_giasEstablishment_typeOfEstablishment'
      AND parent_object_id = OBJECT_ID(N'gias.establishment')
)
BEGIN
    ALTER TABLE gias.establishment
    ADD CONSTRAINT FK_giasEstablishment_typeOfEstablishment
        FOREIGN KEY (typeOfEstablishmentCode) REFERENCES gias.typeOfEstablishment (typeOfEstablishmentCode);
END;
GO
