using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.UnitTests.Shared.Builders;

public class EntityBuilders
{
    public static AnswerEntity BuildAnswer(int id = 1, string? answerRef = null) =>
        new()
        {
            Id = id,
            ContentfulRef = answerRef ?? $"A{id:000}",
            AnswerText = $"Answer {id}",
        };

    public static EstablishmentEntity BuildEstablishment(
        int establishmentId = 1,
        DateTime? dateCreated = null
    ) =>
        new()
        {
            Id = establishmentId,
            EstablishmentRef = $"testRef{establishmentId}",
            OrgName = $"testName{establishmentId}",
            DateCreated = dateCreated ?? DateTime.UtcNow,
        };

    public static QuestionEntity BuildQuestion(int id = 1, string? questionRef = null) =>
        new()
        {
            Id = id,
            ContentfulRef = questionRef ?? $"Q{id:000}",
            QuestionText = $"Question {id}",
        };

    public static RecommendationEntity BuildRecommendation(
        int id,
        string contentfulRef,
        string? recommendationText = null
    ) =>
        new()
        {
            Id = id,
            ContentfulRef = contentfulRef,
            RecommendationText = recommendationText ?? $"Recommendation {id}",
        };

    public static ResponseEntity BuildResponse(
        int id,
        int userId,
        int userEstablishmentId,
        int submissionId,
        SubmissionEntity? submission,
        int questionId,
        string? questionRef,
        int answerId,
        string? answerRef,
        DateTime? dateCreated = null,
        DateTime? dateLastUpdated = null
    )
    {
        return new ResponseEntity
        {
            Id = id,
            DateCreated = dateCreated ?? DateTime.UtcNow,
            DateLastUpdated = dateLastUpdated ?? DateTime.UtcNow,
            UserId = userId,
            UserEstablishmentId = userEstablishmentId,
            SubmissionId = submissionId,
            Submission = submission ?? null!,
            QuestionId = questionId,
            Question = BuildQuestion(questionId, questionRef),
            AnswerId = answerId,
            Answer = BuildAnswer(answerId, answerRef),
        };
    }

    public static SubmissionEntity BuildEmptySubmission() =>
        BuildSubmission(id: 0, establishmentId: 1, sectionId: "SEC000");

    public static SubmissionEntity BuildSubmission(
        int? id,
        int? establishmentId,
        string? sectionId,
        int? dateCreatedDaysBefore = null,
        int? dateLastUpdatedDaysBefore = null,
        int? dateCompletedDaysBefore = null,
        List<ResponseEntity>? responses = null,
        SubmissionStatus? submissionStatus = SubmissionStatus.CompleteReviewed
    ) =>
        new()
        {
            Id = id ?? 0,
            EstablishmentId = establishmentId ?? 0,
            Establishment = BuildEstablishment(establishmentId ?? 0),
            SectionId = $"S{sectionId:000}",
            SectionName = $"Test Section {sectionId}",
            Responses = responses ?? [],
            DateCreated = DateTime.Now.AddDays(-dateCreatedDaysBefore ?? 0),
            DateLastUpdated = DateTime.Now.AddDays(-dateLastUpdatedDaysBefore ?? 0),
            DateCompleted = DateTime.Now.AddDays(-dateCompletedDaysBefore ?? 0),
            Status = submissionStatus ?? SubmissionStatus.None,
        };

    public static ResponseEntity BuildResponse(
        int id,
        DateTime dateCreated,
        SubmissionEntity submission,
        int questionId,
        string questionRef,
        int answerId,
        string answerRef
    ) =>
        new()
        {
            Id = id,
            DateCreated = dateCreated,
            DateLastUpdated = dateCreated,
            QuestionId = id * 10,
            Question = BuildQuestion(questionId, questionRef),
            Answer = BuildAnswer(answerId, answerRef),
            SubmissionId = submission.Id,
            Submission = submission,
        };

    public static UserEntity BuildUser(int id) => new() { DfeSignInRef = $"User{id}" };
}
