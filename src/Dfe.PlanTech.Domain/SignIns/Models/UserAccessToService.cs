using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.SignIns.Models;

public sealed class UserAccessToService
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("serviceId")]
    public Guid ServiceId { get; set; }

    [JsonPropertyName("organisationId")]
    public Guid OrganisationId { get; set; }

    [JsonPropertyName("roles")]
    public IList<Role> Roles { get; set; } = null!;

    [JsonPropertyName("identifiers")]
    public IList<Identifier> Identifiers { get; set; } = null!;

}
