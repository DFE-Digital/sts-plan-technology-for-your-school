using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.SignIns.Models;

public sealed class Status
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
