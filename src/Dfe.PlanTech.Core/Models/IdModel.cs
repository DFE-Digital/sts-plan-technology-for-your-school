using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class IdModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}
