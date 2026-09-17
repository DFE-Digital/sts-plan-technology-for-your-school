using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class AssessmentResponseModel
{
    public int UserId { get; set; }

    public int UserEstablishmentId { get; set; }

    public int EstablishmentId { get; set; }

    public int SubmissionId { get; set; }

    public string SectionId { get; set; }

    public string SectionName { get; set; }

    public int QuestionId { get; set; }

    public int AnswerId { get; set; }

    public IdWithTextModel Question { get; init; }

    public IdWithTextModel? Answer { get; init; }

    public AssessmentResponseModel(
        int userId,
        int activeEstablishmentId,
        int userEstablishmentId,
        int dbSubmissionId,
        int dbQuestionId,
        int dbAnswerid,
        SubmitAnswerModel questionAnswer
    )
    {
        UserId = userId;
        UserEstablishmentId = userEstablishmentId;
        EstablishmentId = activeEstablishmentId;
        SubmissionId = dbSubmissionId;
        SectionId = questionAnswer.SectionId;
        SectionName = questionAnswer.SectionName;
        QuestionId = dbQuestionId;
        AnswerId = dbAnswerid;
        Question = questionAnswer.Question;
        Answer = questionAnswer.ChosenAnswer;
    }
}
