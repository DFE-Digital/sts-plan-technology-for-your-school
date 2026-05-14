using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class IdWithSlugAndTextModel : IdWithTextModel
{
    [JsonPropertyName("slug")]
    public string Slug { get; init; } = null!;
}
