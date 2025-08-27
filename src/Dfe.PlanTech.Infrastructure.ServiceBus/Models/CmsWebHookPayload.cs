using System.Text.Json.Nodes;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Models;

public class CmsWebHookPayload
{
    public string Id => Sys.Id;
    public Dictionary<string, JsonNode> Fields { get; init; } = null!;
    public CmsWebHookSystemDetails Sys { get; init; } = null!;

    public string ContentType => Sys.Type;
}
