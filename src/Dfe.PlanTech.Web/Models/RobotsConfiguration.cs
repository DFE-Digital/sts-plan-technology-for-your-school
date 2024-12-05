namespace Dfe.PlanTech.Web.Models;

public record RobotsConfiguration
{
    public string UserAgent { get; init; } = "*";
    public string[] DisallowedPaths { get; init; } = [];
    public int CacheMaxAge { get; init; } = 99999;
}