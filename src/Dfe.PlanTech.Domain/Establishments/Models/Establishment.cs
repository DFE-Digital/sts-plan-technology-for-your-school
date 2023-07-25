namespace Dfe.PlanTech.Domain.Establishments.Models
{
    public class Establishment
    {
        public int Id { get; set; }
        public string EstablishmentRef { get; set; } = null!;

        public string EstablishmentType { get; set; } = null!;

        public string OrgName { get; set; } = null!;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateLastUpdated { get; set; }
    }
}
