SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetFirstActivityForEstablishmentRecommendation]
  @EstablishmentId INT,
  @RecommendationContentfulRef VARCHAR(50)
AS
BEGIN TRY
  ;WITH history AS
  (
    SELECT TOP 1
      rh.dateCreated,
      rh.establishmentId,
      rh.matEstablishmentId,
      rh.newStatus,
      rh.recommendationId,
      rec.contentfulRef,
      rec.questionId,
      sr.sectionRef
    FROM
      dbo.establishmentRecommendationHistory rh
      JOIN dbo.recommendation rec
        ON rec.id = rh.recommendationId
      JOIN migration.sectionRecommendations sr
        ON sr.recommendationRef = rec.contentfulRef
    WHERE
	    rh.previousStatus IS NULL
      AND rh.establishmentId = @establishmentId
      AND rec.contentfulRef = @recommendationContentfulRef
    ORDER BY
	    rh.id DESC
  )

  SELECT TOP 1
    history.dateCreated AS statusChangeDate,
    history.newStatus AS statusText,
    school.orgName AS schoolName,
    mat.orgName AS groupName,
    resp.userId,
    q.questionText,
    a.answerText
  FROM
    history
    JOIN dbo.submission s
      ON s.establishmentId = history.establishmentId
      AND s.sectionId = history.sectionRef
    JOIN dbo.establishment school
      ON school.id = history.establishmentId
    LEFT JOIN dbo.establishment mat
      ON mat.id = history.matEstablishmentId
    JOIN dbo.response resp
      ON resp.submissionId = s.id
      AND resp.questionId = history.questionId
    JOIN dbo.question q
      ON q.id = resp.questionId
    JOIN dbo.answer a
      ON a.id = resp.answerId

END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH

GO
