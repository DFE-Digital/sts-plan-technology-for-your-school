using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class IdWithTextModel : IdModel
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;
}
