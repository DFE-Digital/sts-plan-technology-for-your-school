using Dfe.PlanTech.Domain.Submissions.Enums;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public class SectionStatusDto
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? LastMaturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }
}

public record SectionStatusNew
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }

    public Status Status { get; init; }
}
