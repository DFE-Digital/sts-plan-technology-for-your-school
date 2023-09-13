namespace Dfe.PlanTech.Domain.Submissions.Models;

public class Submission
{
    public int Id { get; set; }

    public int EstablishmentId { get; set; }

    public bool Completed { get; set; }

    public required string SectionId { get; set; }

    public required string SectionName { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateLastUpdated { get; set; }

    public DateTime? DateCompleted { get; set; }
}
