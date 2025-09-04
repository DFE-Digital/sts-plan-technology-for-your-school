using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Workflows.Options;

[ExcludeFromCodeCoverage]
public record CookieWorkflowOptions
{
    public required string[] EssentialCookies { get; init; }
}
