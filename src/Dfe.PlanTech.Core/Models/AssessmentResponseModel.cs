namespace Dfe.PlanTech.Core.Models;

public class AssessmentResponseModel
{
    public int UserId { get; set; }

    public int UserEstablishmentId { get; set; }

    public int EstablishmentId { get; set; }

    public string SectionId { get; set; }

    public string SectionName { get; set; }

    public IdWithTextModel Question { get; init; }

    public IdWithTextModel? Answer { get; init; }

    public AssessmentResponseModel(int userId, int establishmentId, int? matEstablishmentId, SubmitAnswerModel questionAnswer)
    {
        SectionId = questionAnswer.SectionId;
        SectionName = questionAnswer.SectionName;
        Answer = questionAnswer.ChosenAnswer;
        Question = questionAnswer.Question;
        EstablishmentId = establishmentId;
        UserId = userId;
        UserEstablishmentId = matEstablishmentId ?? establishmentId;
    }
}
