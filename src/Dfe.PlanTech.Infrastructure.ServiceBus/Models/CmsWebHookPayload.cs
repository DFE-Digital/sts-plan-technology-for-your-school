using System.Text.Json.Nodes;

namespace Dfe.PlanTech.Domain.Caching.Models;

public class CmsWebHookPayload
{
    public Dictionary<string, JsonNode> Fields { get; init; } = null!;
    public CmsWebHookSystemDetails Sys { get; init; } = null!;

    public string ContentType => Sys.ContentType.Sys.Id;
}
