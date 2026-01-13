using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

public class IdModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}
