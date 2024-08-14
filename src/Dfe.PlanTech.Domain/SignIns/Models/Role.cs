using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.SignIns.Models;

public class Role
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("numericId")]
    public string NumericId { get; set; } = null!;

    [JsonPropertyName("status")]
    public Status Status { get; set; } = null!;

}
