namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class SubmissionDto
{
    public int Id { get; set; }

    public int EstablishmentId { get; set; }

    public EstablishmentDto Establishment { get; set; } = null!;

    public bool Completed { get; set; }

    public required string SectionId { get; set; }

    public required string SectionName { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateLastUpdated { get; set; }

    public DateTime? DateCompleted { get; set; }

    public List<ResponseDto> Responses { get; set; } = new();

    public bool Deleted { get; set; }

    public bool Viewed { get; set; }

    public string? Status { get; set; }
}
