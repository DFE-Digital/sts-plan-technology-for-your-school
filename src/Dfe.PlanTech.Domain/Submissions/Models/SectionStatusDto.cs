namespace Dfe.PlanTech.Domain.Submissions.Models;

public class SectionStatusDto
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? LastMaturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }
}
