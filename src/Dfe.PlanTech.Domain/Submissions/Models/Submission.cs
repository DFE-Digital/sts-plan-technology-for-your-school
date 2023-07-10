namespace Dfe.PlanTech.Domain.Submissions.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int EastablishmentId { get; set; }

        public bool Completed { get; set; }

        public string SectionId { get; set; } = null!;

        public string SectionName { get; set; } = null!;

        public string Maturity { get; set; } = null!;

        public int RecomendationId { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateLastUpdated { get; set; }

        public DateTime DateCompleted { get; set; }
    }
}
