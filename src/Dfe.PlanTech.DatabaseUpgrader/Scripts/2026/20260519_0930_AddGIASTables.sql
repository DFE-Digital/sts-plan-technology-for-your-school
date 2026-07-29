-- ============================================================
-- GIAS Schema - Normalized
-- ============================================================
-- Tables are listed in dependency order (lookup tables first).
--
-- Key ID disambiguation:
--   URN         INT   Unique Reference Number. Always populated.
--                     Primary key for every establishment.
--                     DSI: organisation.urn
--
--   Group UID   INT   Unique Identifier for groups (MATs, federations, etc.).
--                     Always populated for groups.
--                     DSI: organisation.uid
--
--   UKPRN       INT   UK Provider Reference Number.
--                     ~90% populated for establishments, 43% for groups.
--                     Useful as a secondary DSI lookup.
--
--   Group ID    STR   DfE-assigned identifier (e.g. "TR01234").
--                     ~70% populated. Not a reliable primary key.
--
--
-- Relationship to existing dbo tables (migration guide):
--   dbo.establishment.establishmentRef  → gias.establishment.urn       (schools)
--                                       → gias.establishmentGroup.groupUid (MATs)
--   dbo.establishmentGroup.uid          → gias.establishmentGroup.groupUid (CAST to INT)
--   dbo.establishmentLink.urn           → gias.establishment.urn       (CAST to INT)
--   dbo.establishmentRecommendationHistory.matEstablishmentId
--       currently a FK into dbo.establishment (the conflation problem);
--       will become a direct reference to gias.establishmentGroup.groupUid
--
-- No PII is stored: head teacher names and governance records are excluded.
-- ============================================================

CREATE SCHEMA gias;
GO


-- ============================================================
-- ---- Lookup / reference tables (establishment) ----
-- ============================================================

CREATE TABLE gias.establishmentStatus (
    establishmentStatusCode  INT             NOT NULL,
    establishmentStatusName  NVARCHAR(50)    NOT NULL,   -- "Open" / "Closed" / "Proposed" / etc.
    CONSTRAINT PK_giasEstablishmentStatus PRIMARY KEY (establishmentStatusCode)
);
GO

CREATE TABLE gias.gender (
    genderCode  INT            NOT NULL,
    genderName  NVARCHAR(50)   NOT NULL,   -- "Mixed" / "Boys" / "Girls" / "Not applicable"
    CONSTRAINT PK_giasGender PRIMARY KEY (genderCode)
);
GO

CREATE TABLE gias.localAuthority (
    localAuthorityCode      INT            NOT NULL,   -- 3-digit code
    localAuthorityName      NVARCHAR(255)  NOT NULL,
    CONSTRAINT PK_giasLocalAuthority PRIMARY KEY (localAuthorityCode)
);
GO

CREATE TABLE gias.phase (
    phaseCode  INT            NOT NULL,
    phaseName  NVARCHAR(50)   NOT NULL,   -- "Primary" / "Secondary" / "All-through" / "16 plus" etc.
    CONSTRAINT PK_giasPhase PRIMARY KEY (phaseCode)
);
GO

-- ============================================================
-- ---- Lookup / reference tables (group) ----
-- ============================================================

CREATE TABLE gias.groupStatus (
    groupStatusCode    NVARCHAR(50)    NOT NULL,
    groupStatusName    NVARCHAR(50)    NOT NULL,   -- "Open" / "Closed" / etc.
    CONSTRAINT PK_giasGroupStatus PRIMARY KEY (groupStatusCode)
);
GO

-- Group types: "Multi-academy trust", "Federation", "Single-academy trust", etc.
CREATE TABLE gias.groupType (
    groupTypeCode  INT             NOT NULL,
    groupTypeName  NVARCHAR(100)   NOT NULL,
    CONSTRAINT PK_giasGroupType PRIMARY KEY (groupTypeCode)
);
GO


-- ============================================================
-- ---- Core entity tables ----
-- ============================================================

-- ============================================================
-- gias.establishment
-- Source: edubasealldata<YYYYMMDD>.csv
-- Identity, classification, and metadata for all schools and colleges.
-- Group links → gias.groupMembership
-- ============================================================
CREATE TABLE gias.establishment (
    -- --------------------------------------------------------
    -- Identity
    -- --------------------------------------------------------
    urn                     INT             NOT NULL,
    uprn                    BIGINT          NULL,       -- Unique Property Reference Number (up to 12 digits)
    ukprn                   INT             NULL,       -- 90% populated; secondary DSI lookup
    establishmentNumber     INT             NULL,       -- 4-digit DfE number within LA; 99% populated
    establishmentName       NVARCHAR(255)   NOT NULL,

    -- --------------------------------------------------------
    -- Coded values (FK)
    -- --------------------------------------------------------
    establishmentStatusCode INT             NOT NULL
        CONSTRAINT FK_giasEstablishment_status
        FOREIGN KEY (establishmentStatusCode) REFERENCES gias.establishmentStatus (establishmentStatusCode),

    genderCode              INT             NOT NULL,
    CONSTRAINT FK_giasEstablishment_gender
        FOREIGN KEY (genderCode) REFERENCES gias.gender (genderCode),

    localAuthorityCode      INT             NOT NULL
        CONSTRAINT FK_giasEstablishment_la
        FOREIGN KEY (localAuthorityCode) REFERENCES gias.localAuthority (localAuthorityCode),

    phaseCode               INT             NOT NULL,
    CONSTRAINT FK_giasEstablishment_phase
        FOREIGN KEY (phaseCode) REFERENCES gias.phase (phaseCode),

    -- --------------------------------------------------------
    -- Sync metadata
    -- --------------------------------------------------------
    syncedAt                DATETIME2       NOT NULL    CONSTRAINT DF_giasEstablishment_syncedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_giasEstablishment PRIMARY KEY CLUSTERED (urn)
);
GO

CREATE INDEX IX_giasEstablishment_uprn
    ON gias.establishment (uprn) WHERE uprn IS NOT NULL;
GO

CREATE INDEX IX_giasEstablishment_ukprn
    ON gias.establishment (ukprn) WHERE ukprn IS NOT NULL;
GO

CREATE INDEX IX_giasEstablishment_gender
    ON gias.establishment (genderCode);
GO

CREATE INDEX IX_giasEstablishment_phase
    ON gias.establishment (phaseCode);
GO


-- ============================================================
-- gias.establishmentGroup
-- Source: allgroupsdata<YYYYMMDD>.csv
-- MATs, SATs, federations, trusts, school sponsors, etc.
-- Includes closed groups so historical membership stays joinable.
--
-- DSI lookup: organisation.uid → groupUid (INT).
-- ============================================================
CREATE TABLE gias.establishmentGroup (
    -- --------------------------------------------------------
    -- Identity
    -- --------------------------------------------------------
    groupUid                INT             NOT NULL,
    groupId                 NVARCHAR(100)   NULL,       -- e.g. "TR01234"; ~70% populated; not a reliable key
    ukprn                   INT             NULL,       -- ~45% populated; secondary DSI lookup
    groupName               NVARCHAR(255)   NOT NULL,

    -- --------------------------------------------------------
    -- Coded values (FK)
    -- --------------------------------------------------------
    groupStatusCode         NVARCHAR(50)    NOT NULL
        CONSTRAINT FK_giasEstablishmentGroup_status
        FOREIGN KEY (groupStatusCode) REFERENCES gias.groupStatus (groupStatusCode),

    groupTypeCode                INT        NOT NULL,
    CONSTRAINT FK_giasEstablishmentGroup_type
        FOREIGN KEY (groupTypeCode) REFERENCES gias.groupType (groupTypeCode),

    -- --------------------------------------------------------
    -- Sync metadata
    -- --------------------------------------------------------
    syncedAt                DATETIME2       NOT NULL    CONSTRAINT DF_giasEstablishmentGroup_syncedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_giasEstablishmentGroup PRIMARY KEY CLUSTERED (groupUid)
);
GO

CREATE INDEX IX_giasEstablishmentGroup_groupId
    ON gias.establishmentGroup (groupId) WHERE groupId IS NOT NULL;
GO

CREATE INDEX IX_giasEstablishmentGroup_ukprn
    ON gias.establishmentGroup (ukprn) WHERE ukprn IS NOT NULL;
GO

CREATE INDEX IX_giasEstablishmentGroup_groupTypeCode
    ON gias.establishmentGroup (groupTypeCode);
GO


-- ============================================================
-- ---- LAYER 4: Child / extension tables (all depend on Layer 3) ----
-- ============================================================


-- ============================================================
-- gias.groupMembership
-- Source: alllinksdata (all group types, current snapshot)
--
-- "Which schools are in MAT X?"
--   SELECT urn FROM gias.groupMembership WHERE groupUid = @uid
--
-- "Which MAT is school Y in?"
--   SELECT groupUid FROM gias.groupMembership
--   WHERE urn = @urn AND leftDate IS NULL
--
-- This is the canonical link between establishments and groups.
-- dbo.establishment.groupUid and dbo.establishmentLink are
-- superseded by this table once migration is complete.
-- ============================================================
CREATE TABLE gias.groupMembership (
    id          INT         NOT NULL IDENTITY(1, 1),
    urn         INT         NOT NULL,
    groupUid    INT         NOT NULL,
    syncedAt    DATETIME2   NOT NULL    CONSTRAINT DF_giasGroupMembership_syncedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_giasGroupMembership
        PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_giasGroupMembership_establishment
        FOREIGN KEY (urn) REFERENCES gias.establishment (urn),
    CONSTRAINT FK_giasGroupMembership_group
        FOREIGN KEY (groupUid) REFERENCES gias.establishmentGroup (groupUid)
);
GO

-- One current membership per (school, group) pair.
CREATE UNIQUE INDEX UQ_giasGroupMembership_currentMember
    ON gias.groupMembership (urn, groupUid);
GO

-- Primary pattern: all schools for a given group
CREATE INDEX IX_giasGroupMembership_groupUid
    ON gias.groupMembership (groupUid) INCLUDE (urn);
GO

-- Secondary pattern: all groups for a given school
CREATE INDEX IX_giasGroupMembership_urn
    ON gias.groupMembership (urn) INCLUDE (groupUid);
GO
