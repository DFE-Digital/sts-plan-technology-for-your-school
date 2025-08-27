using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Caching;

[ExcludeFromCodeCoverage]
public record QuestionnaireCache
{
    public string? CurrentSectionTitle { get; init; }
}

