using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class IdWithNameModel : IdModel
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}
