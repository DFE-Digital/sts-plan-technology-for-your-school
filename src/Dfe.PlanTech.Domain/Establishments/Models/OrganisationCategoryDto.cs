using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Establishments.Models
{
    public class OrganisationCategoryDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
