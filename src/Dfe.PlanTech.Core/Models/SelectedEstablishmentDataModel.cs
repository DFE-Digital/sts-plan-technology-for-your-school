using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Models
{
    public class SelectedEstablishmentDataModel
    {
        public int Id { get; set; }
        public required string Urn { get; set; }
        public required string Name { get; set; }
        public SubmissionStatus? LatestSubmissionStatus { get; set; }
    }
}
