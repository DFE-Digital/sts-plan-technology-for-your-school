namespace Dfe.PlanTech.Domain.Submissions.Models;

public class SectionStatus
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }
}
