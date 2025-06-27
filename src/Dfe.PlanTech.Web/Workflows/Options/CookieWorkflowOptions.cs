namespace Dfe.PlanTech.Web.Workflows.Options;

public record CookieWorkflowOptions
{
    public required string[] EssentialCookies { get; init; }
}
