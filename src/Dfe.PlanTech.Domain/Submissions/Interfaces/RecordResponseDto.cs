using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public class RecordResponseDto
{
    public int UserId { get; set; }

    public int EstablishmentId { get; set; }

    public string SectionId { get; set; } = null!;

    public string SectionName { get; set; } = null!;

    public IdWithText Question { get; init; }

    public IdWithText Answer { get; init; }

    public string Maturity { get; set; } = null!;
}
