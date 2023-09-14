namespace Dfe.PlanTech.Domain.Responses.Models;

public class RecordResponseDto
{
    public int UserId { get; set; }

    public int SubmissionId { get; set; }

    public ContentfulReference Question { get; init; }

    public ContentfulReference Answer { get; init; }

    public string Maturity { get; set; } = null!;

    public DateTime? DateCreated { get; set; }
}
