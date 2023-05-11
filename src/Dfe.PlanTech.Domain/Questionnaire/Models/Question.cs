using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Question : IQuestion
{
    public string Text { get; init; } = null!;
}
