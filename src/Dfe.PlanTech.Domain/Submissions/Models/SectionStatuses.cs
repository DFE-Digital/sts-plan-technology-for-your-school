using Newtonsoft.Json;

namespace Dfe.PlanTech.Domain.Submissions.Models
{
    public class SectionStatuses
    {
        [JsonProperty("sectionId")]
        public string SectionId { get; set; } = null!;

        [JsonProperty("completed")]
        public int Completed { get; set; }

        [JsonProperty("maturity")]
        public string Maturity { get; set; } = null!;
    }
}
