namespace Dfe.PlanTech.Application.Configuration;

public record RobotsConfiguration
{
    public string UserAgent { get; init; } = "*";
    public string[] DisallowedPaths { get; init; } = [];
    public int CacheMaxAge { get; init; } = 86400; //One day
}
