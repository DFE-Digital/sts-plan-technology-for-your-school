namespace Dfe.PlanTech.Core.Models;

public class AssessmentResponseModel
{
    public int UserId { get; set; }

    public int EstablishmentId { get; set; }

    public string SectionId { get; set; }

    public string SectionName { get; set; }

    public IdWithTextModel Question { get; init; }

    public IdWithTextModel Answer { get; init; }

    public AssessmentResponseModel(int userId, int establishmentId, SubmitAnswerModel questionAnswer)
    {
        var question = new IdWithTextModel
        {
            Id = questionAnswer.Question.ContentfulSysId,
            Text = questionAnswer.Question.Text
        };

        var answer = new IdWithTextModel
        {
            Id = questionAnswer.ChosenAnswer!.ContentfulSysId,
            Text = questionAnswer.ChosenAnswer!.Text
        };

        SectionId = questionAnswer.SectionId;
        SectionName = questionAnswer.SectionName;
        Answer = answer;
        Question = question;
        EstablishmentId = establishmentId;
        UserId = userId;
    }
}
