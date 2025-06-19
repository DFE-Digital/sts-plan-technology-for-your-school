namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class SectionStatusDto
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? LastMaturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }

    public bool? Viewed { get; set; }

    public DateTime? LastCompletionDate { get; set; }
}
