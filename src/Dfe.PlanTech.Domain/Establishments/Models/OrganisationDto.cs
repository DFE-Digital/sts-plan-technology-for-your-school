using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Establishments.Models
{
    public class OrganisationDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("category")]
        public OrganisationCategoryDto? Category { get; set; }
    }
}
