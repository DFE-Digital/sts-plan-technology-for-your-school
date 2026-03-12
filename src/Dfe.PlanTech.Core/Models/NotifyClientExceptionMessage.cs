using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class NotifyClientExceptionMessage
{
    [JsonPropertyName("errors")]
    public List<NotifyClientExceptionMessageError> Errors { get; set; } = [];

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }
}
