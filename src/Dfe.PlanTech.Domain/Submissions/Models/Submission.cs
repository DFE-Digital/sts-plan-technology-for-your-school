namespace Dfe.PlanTech.Domain.Submissions.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int EastablishmentId { get; set; }

        public bool Completed { get; set; }

        public int SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;

        public string Maturity { get; set; } = string.Empty;

        public int RecomendationId { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateLastUpdated { get; set; }

        public DateTime DateCompleted { get; set; }
    }
}
