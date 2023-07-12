using Newtonsoft.Json;

namespace Dfe.PlanTech.Domain.Establishments.Models
{
    public class Establishment
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
    }
}
