using Dfe.PlanTech.Domain.Caching.Interfaces;

namespace Dfe.PlanTech.Domain.Caching.Models;

public record QuestionnaireCache : IQuestionnaireCache
{
    public string? CurrentSectionTitle { get; init; }
}
