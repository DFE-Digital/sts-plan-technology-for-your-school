-- ============================================================
-- gias.links
-- Source: links_edubasealldata<YYYYMMDD>.csv
-- Links between establishments: mergers, splits,
-- predecessor/successor relationships, etc.
-- ============================================================

CREATE TABLE gias.links (
    -- --------------------------------------------------------
    -- Identity
    -- --------------------------------------------------------
    urn                     INT             NOT NULL
        CONSTRAINT FK_giasLinks_establishment
        FOREIGN KEY (urn) REFERENCES gias.establishment (urn),
    linkedUrn               INT             NOT NULL    -- The URN of the linked establishment
        CONSTRAINT FK_giasLinks_linkedEstablishment
        FOREIGN KEY (linkedUrn) REFERENCES gias.establishment (urn),
    linkType                NVARCHAR(255)   NULL,
    dateLinked              DATE            NULL,

    -- --------------------------------------------------------
    -- Sync metadata
    -- --------------------------------------------------------
    syncedAt                DATETIME2       NOT NULL    CONSTRAINT DF_giasLinks_syncedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_giasLinks PRIMARY KEY CLUSTERED (urn, linkedUrn)
);
GO

CREATE INDEX IX_giasLinks_urn
    ON gias.links (urn);
GO

CREATE INDEX IX_giasLinks_linkedUrn
    ON gias.links (linkedUrn);
GO
