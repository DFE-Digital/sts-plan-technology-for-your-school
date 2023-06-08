namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubmitAnswerDto
{
    public string QuestionId { get; init; } = null!;

    public string ChosenAnswerId { get; init; } = null!;

    public string? NextQuestionId { get; init; }
}
