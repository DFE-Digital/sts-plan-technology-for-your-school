/****** Object:  StoredProcedure [dbo].[GetFirstActivityForEstablishmentRecommendation]    Script Date: 22/07/2026 11:03:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetFirstActivityForEstablishmentRecommendation]
  @EstablishmentId INT,
  @RecommendationContentfulRef VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;

	SELECT TOP 1
		erh.dateCreated AS statusChangeDate,
		erh.newStatus AS statusText,
		school.orgName AS schoolName,
		mat.orgName AS groupName,
		r.userId,
		q.questionText,
		a.answerText
	FROM
	dbo.submission s
		JOIN dbo.response r
			ON r.submissionId = s.id
		JOIN dbo.question q
			ON q.id = r.questionId
		JOIN dbo.answer a
			ON a.id = r.answerId
		JOIN dbo.recommendation rec
			ON rec.questionContentfulRef = q.contentfulRef
		JOIN dbo.establishmentRecommendationHistory erh
			ON erh.recommendationId = rec.id
			AND erh.establishmentId = s.establishmentId
		JOIN dbo.establishment school
			ON school.id = erh.establishmentId
		LEFT JOIN dbo.establishment mat
			ON mat.id = erh.matEstablishmentId
	WHERE
		s.establishmentId = @EstablishmentId
		AND s.status = 'CompleteReviewed'
		AND rec.contentfulRef = @RecommendationContentfulRef
	ORDER BY
		s.id DESC;

END
