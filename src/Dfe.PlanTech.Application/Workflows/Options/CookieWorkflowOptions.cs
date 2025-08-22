namespace Dfe.PlanTech.Application.Workflows.Options;

public record CookieWorkflowOptions
{
    public required string[] EssentialCookies { get; init; }
}
