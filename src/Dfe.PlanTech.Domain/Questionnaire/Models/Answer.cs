using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Answer : ContentComponent, IAnswer<Question>
{
    /// <summary>
    /// Actual answer
    /// </summary>
    /// <value></value>
    public string Text { get; init; } = null!;

    public Question? NextQuestion { get; init; }

    public string Maturity { get; init; } = null!;

    public AnswerViewModelDto AsDto => new(this);
}
