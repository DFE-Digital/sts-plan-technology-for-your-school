using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Domain.Submissions.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int EstablishmentId { get; set; }

        public Establishment Establishment { get; set; } = null!;
        
        public bool Completed { get; set; }

        public string SectionId { get; set; } = null!;

        public string SectionName { get; set; } = null!;

        public string? Maturity { get; set; }

        public int RecomendationId { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateLastUpdated { get; set; }

        public DateTime? DateCompleted { get; set; }

        public List<Response> Responses { get; set; } = new List<Response>();
    }
}
