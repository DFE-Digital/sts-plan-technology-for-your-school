using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Answer : ContentComponent
{
    /// <summary>
    /// Actual answer
    /// </summary>
    /// <value></value>
    public string Text { get; init; } = null!;

    public Question? NextQuestion { get; init; }

    public string Maturity { get; init; } = null!;

    public string RadioValue => JsonSerializer.Serialize(new
    {
        Sys.Id,
        Text,
        Maturity,
        NextQuestion = NextQuestion != null ? new
        {
            NextQuestion?.Slug,
            NextQuestion?.Sys.Id
        } : null
    });

    public static Answer? FromJson(string json) => JsonSerializer.Deserialize<Answer>(json);
}