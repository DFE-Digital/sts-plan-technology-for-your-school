-- ============================================================
-- GIAS Schema - Normalized
-- ============================================================
-- Adds new columns to gias.establishment to support the GIAS
-- import process. Creates additional lookup / reference tables
-- to support the new columns.
-- ============================================================

-- ============================================================
-- ---- Lookup / reference tables (establishment) ----
-- ============================================================

CREATE TABLE gias.administrativeDistrict (
    administrativeDistrictCode  NVARCHAR(16)    NOT NULL,
    administrativeDistrictName  NVARCHAR(255)   NOT NULL,
    CONSTRAINT PK_giasAdministrativeDistrict PRIMARY KEY (administrativeDistrictCode)
);
GO

CREATE TABLE gias.administrativeWard (
    administrativeWardCode  NVARCHAR(16)    NOT NULL,
    administrativeWardName  NVARCHAR(127)   NOT NULL,
    CONSTRAINT PK_giasAdministrativeWard PRIMARY KEY (administrativeWardCode)
);
GO

CREATE TABLE gias.admissionsPolicy (
    admissionsPolicyCode  INT             NOT NULL,
    admissionsPolicyName  NVARCHAR(127)   NOT NULL,
    CONSTRAINT PK_giasAdmissionsPolicy PRIMARY KEY (admissionsPolicyCode)
);
GO

CREATE TABLE gias.establishmentTypeGroup (
    establishmentTypeGroupCode  INT             NOT NULL,
    establishmentTypeGroupName  NVARCHAR(127)   NOT NULL,
    CONSTRAINT PK_giasEstablishmentTypeGroup PRIMARY KEY (establishmentTypeGroupCode)
);
GO

CREATE TABLE gias.governmentOfficeRegion (
    governmentOfficeRegionCode  NVARCHAR(4)     NOT NULL,
    governmentOfficeRegionName  NVARCHAR(127)   NOT NULL,
    CONSTRAINT PK_giasGovernmentOfficeRegion PRIMARY KEY (governmentOfficeRegionCode)
);
GO

CREATE TABLE gias.parliamentaryConstituency (
     parliamentaryConstituencyCode  NVARCHAR(16)    NOT NULL,
     parliamentaryConstituencyName  NVARCHAR(127)   NOT NULL,
   CONSTRAINT PK_giasParliamentaryConstituency PRIMARY KEY (parliamentaryConstituencyCode)
)

CREATE TABLE gias.religiousCharacter (
   religiousCharacterCode  INT             NOT NULL,
   religiousCharacterName  NVARCHAR(127)   NOT NULL,
   CONSTRAINT PK_giasReligiousCharacter PRIMARY KEY (religiousCharacterCode)
);
GO

CREATE TABLE gias.sixthFormStatus (
   sixthFormStatusCode  INT             NOT NULL,
   sixthFormStatusName  NVARCHAR(127)   NOT NULL,
   CONSTRAINT PK_giasSixthFormStatus PRIMARY KEY (sixthFormStatusCode)
);
GO

CREATE TABLE gias.trust (
   trustCode  INT             NOT NULL,
   trustName  NVARCHAR(127)   NOT NULL,
   CONSTRAINT PK_giasTrust PRIMARY KEY (trustCode)
);
GO

CREATE TABLE gias.trustSchoolFlag (
   trustSchoolFlagCode  INT             NOT NULL,
   trustSchoolFlagName  NVARCHAR(127)   NOT NULL,
   CONSTRAINT PK_giasTrustSchoolFlag PRIMARY KEY (trustSchoolFlagCode)
);
GO

CREATE TABLE gias.urbanRuralClassification (
   urbanRuralCode  NVARCHAR(8)     NOT NULL,
   urbanRuralName  NVARCHAR(127)   NOT NULL,
   CONSTRAINT PK_giasUrbanRuralClassification PRIMARY KEY (urbanRuralCode)
);
GO

-- ============================================================
-- ---- gias.establishment table ----
-- ============================================================
--
-- Template:
-- ALTER TABLE gias.establishment
-- ADD <tableName>Code <type> NOT NULL
--     CONSTRAINT FK_giasEstablishment_<tableName>
--     FOREIGN KEY (<tableName>Code) REFERENCES gias.<tableName> (<tableName>Code)
--
-- CREATE INDEX IX_giasEstablishment_<tableName>
-- ON gias.establishment (<tableName>Code)
-- WHERE <tableName>Code IS NOT NULL;
-- GO

-- --------------------------------------------------------
-- Coded values (FK)
-- --------------------------------------------------------

ALTER TABLE gias.establishment
ADD administrativeDistrictCode NVARCHAR(16) NULL
    CONSTRAINT FK_giasEstablishment_administrativeDistrict
    FOREIGN KEY (administrativeDistrictCode) REFERENCES gias.administrativeDistrict (administrativeDistrictCode);

ALTER TABLE gias.establishment
ADD administrativeWardCode NVARCHAR(16) NULL
    CONSTRAINT FK_giasEstablishment_administrativeWard
    FOREIGN KEY (administrativeWardCode) REFERENCES gias.administrativeWard (administrativeWardCode);

ALTER TABLE gias.establishment
ADD admissionsPolicyCode INT NULL
    CONSTRAINT FK_giasEstablishment_admissionsPolicy
    FOREIGN KEY (admissionsPolicyCode) REFERENCES gias.admissionsPolicy (admissionsPolicyCode);

ALTER TABLE gias.establishment
ADD establishmentTypeGroupCode INT NULL
    CONSTRAINT FK_giasEstablishment_establishmentTypeGroup
    FOREIGN KEY (establishmentTypeGroupCode) REFERENCES gias.establishmentTypeGroup (establishmentTypeGroupCode);

ALTER TABLE gias.establishment
ADD governmentOfficeRegionCode NVARCHAR(4) NULL
    CONSTRAINT FK_giasEstablishment_governmentOfficeRegion
    FOREIGN KEY (governmentOfficeRegionCode) REFERENCES gias.governmentOfficeRegion (governmentOfficeRegionCode);

ALTER TABLE gias.establishment
ADD parliamentaryConstituencyCode NVARCHAR(16) NULL
    CONSTRAINT FK_giasEstablishment_parliamentaryConstituency
    FOREIGN KEY (parliamentaryConstituencyCode) REFERENCES gias.parliamentaryConstituency (parliamentaryConstituencyCode);

ALTER TABLE gias.establishment
ADD religiousCharacterCode INT NULL
    CONSTRAINT FK_giasEstablishment_religiousCharacter
    FOREIGN KEY (religiousCharacterCode) REFERENCES gias.religiousCharacter (religiousCharacterCode);

ALTER TABLE gias.establishment
ADD sixthFormStatusCode INT NULL
    CONSTRAINT FK_giasEstablishment_sixthFormStatus
    FOREIGN KEY (sixthFormStatusCode) REFERENCES gias.sixthFormStatus (sixthFormStatusCode);

ALTER TABLE gias.establishment
ADD trustCode INT NULL
    CONSTRAINT FK_giasEstablishment_trust
    FOREIGN KEY (trustCode) REFERENCES gias.trust (trustCode);

ALTER TABLE gias.establishment
ADD trustSchoolFlagCode INT NULL
    CONSTRAINT FK_giasEstablishment_trustSchoolFlag
    FOREIGN KEY (trustSchoolFlagCode) REFERENCES gias.trustSchoolFlag (trustSchoolFlagCode);

ALTER TABLE gias.establishment
ADD urbanRuralCode NVARCHAR(8) NULL
    CONSTRAINT FK_giasEstablishment_urbanRuralClassification
    FOREIGN KEY (urbanRuralCode) REFERENCES gias.urbanRuralClassification (urbanRuralCode);

GO
-- --------------------------------------------------------
-- Indexes
-- --------------------------------------------------------

CREATE INDEX IX_giasEstablishment_administrativeDistrict
ON gias.establishment (administrativeDistrictCode);
GO

CREATE INDEX IX_giasEstablishment_establishmentTypeGroup
ON gias.establishment (establishmentTypeGroupCode);
GO

CREATE INDEX IX_giasEstablishment_governmentOfficeRegion
ON gias.establishment (governmentOfficeRegionCode);
GO

CREATE INDEX IX_giasEstablishment_trust
ON gias.establishment (trustCode)
WHERE trustCode IS NOT NULL;
GO

CREATE INDEX IX_giasEstablishment_typeOfEstablishment
ON gias.establishment (typeOfEstablishmentCode)
GO
