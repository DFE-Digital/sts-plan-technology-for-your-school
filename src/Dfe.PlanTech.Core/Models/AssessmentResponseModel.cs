namespace Dfe.PlanTech.Core.RoutingDataModel;

public class AssessmentResponseModel
{
    public int UserId { get; set; }

    public int EstablishmentId { get; set; }

    public string SectionId { get; set; } = null!;

    public string SectionName { get; set; } = null!;

    public IdWithTextModel Question { get; init; }

    public IdWithTextModel Answer { get; init; }

    public string Maturity { get; set; } = null!;

    public AssessmentResponseModel(int userId, int establishmentId, SubmitAnswerModel questionAnswer)
    {
        SectionId = questionAnswer.SectionId;
        SectionName = questionAnswer.SectionName;
        Answer = questionAnswer.ChosenAnswer!.Answer;
        Question = new IdWithTextModel
        {
            Id = questionAnswer.QuestionId,
            Text = questionAnswer.QuestionText
        };
        Maturity = questionAnswer.ChosenAnswer.Maturity;
        EstablishmentId = establishmentId;
        UserId = userId;
    }
}
