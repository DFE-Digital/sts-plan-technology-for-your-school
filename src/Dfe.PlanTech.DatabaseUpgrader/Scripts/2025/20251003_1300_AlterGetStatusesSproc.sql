USE [plantech]
GO

/****** Object:  StoredProcedure [dbo].[GetSectionStatuses]    Script Date: 03-Oct-25 13:00:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetSectionStatuses] 
    @sectionIds NVARCHAR(MAX), 
    @establishmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Split sectionIds into a temp table
    SELECT Value AS sectionId
    INTO #SectionIds
    FROM STRING_SPLIT(@sectionIds, ',');

    SELECT
        CurrentSubmission.sectionId,
        CurrentSubmission.completed,
        LastCompleteSubmission.maturity       AS lastMaturity,
        CurrentSubmission.dateCreated,
        CurrentSubmission.dateLastUpdated     AS dateUpdated,
        LastCompleteSubmission.viewed,
        LastCompleteSubmission.dateCompleted  AS lastCompletionDate,
        E.orgName                             AS orgName   -- added column
    FROM #SectionIds SI

    -- Current submission (most recent)
    CROSS APPLY (
        SELECT TOP 1
            sectionId,
            completed,
            S.id,
            dateCreated,
            dateLastUpdated
        FROM [dbo].submission S
        WHERE
              SI.sectionId = S.sectionId
          AND S.establishmentId = @establishmentId
          AND S.deleted = 0
        ORDER BY S.dateCreated DESC
    ) CurrentSubmission

    -- Most recent complete submission (if there is one)
    OUTER APPLY (
        SELECT TOP 1
            maturity,
            viewed,
            dateCompleted
        FROM [dbo].submission S
        WHERE
              SI.sectionId = S.sectionId
          AND S.establishmentId = @establishmentId
          AND S.deleted = 0
          AND S.completed = 1
        ORDER BY S.dateCreated DESC
    ) LastCompleteSubmission

    -- Join establishment to pull orgName
    INNER JOIN dbo.establishment E
        ON E.id = @establishmentId;

    DROP TABLE #SectionIds;
END
GO
