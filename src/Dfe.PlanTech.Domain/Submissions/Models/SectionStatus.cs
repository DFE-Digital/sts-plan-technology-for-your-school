using Dfe.PlanTech.Domain.Submissions.Enums;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public record SectionStatus
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }

    public Status Status { get; init; }
}
