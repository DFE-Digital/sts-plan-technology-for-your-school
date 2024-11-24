using System.Text.Json.Serialization;
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

    [JsonIgnore]
    public Question? NextQuestion { get; init; }

    public string Maturity { get; init; } = null!;

    public AnswerViewModelDto AsDto => new(this);


    private string? _nextQuestionId;

    /// <summary>
    /// Serialize just the next Question Id as JSON to prevent infinite recursive paths
    /// </summary>
    public string? NextQuestionId
    {
        get => _nextQuestionId ?? NextQuestion?.Sys.Id;
        set => _nextQuestionId = value;
    }
}
