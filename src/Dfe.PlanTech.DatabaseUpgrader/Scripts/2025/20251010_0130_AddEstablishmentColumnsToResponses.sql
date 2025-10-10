ALTER TABLE dbo.response
ADD userEstablishmentId INT NOT NULL
GO

-- FK
ALTER TABLE dbo.response
    ADD CONSTRAINT FK_response_userEstablishment
        FOREIGN KEY (userEstablishmentId) REFERENCES dbo.[establishment](id);
GO
