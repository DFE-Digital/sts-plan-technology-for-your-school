-- Migration to fix establishmentRecommendationHistory table for append-only design
-- This script should be run after the existing migration

-- Drop the existing primary key constraint
ALTER TABLE dbo.establishmentRecommendationHistory
    DROP CONSTRAINT PK_establishmentRecommendationHistory;
GO

-- Add an identity column as the new primary key
ALTER TABLE dbo.establishmentRecommendationHistory
    ADD id INT IDENTITY(1,1) NOT NULL;
GO

-- Create the new primary key on the identity column
ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT PK_establishmentRecommendationHistory PRIMARY KEY (id);
GO

-- Create an index on the original composite key for performance
CREATE INDEX IX_establishmentRecommendationHistory_EstablishmentRecommendation
    ON dbo.establishmentRecommendationHistory (establishmentId, recommendationId);
GO

-- Create an index on dateCreated for ordering by time
CREATE INDEX IX_establishmentRecommendationHistory_DateCreated
    ON dbo.establishmentRecommendationHistory (dateCreated DESC);
GO
