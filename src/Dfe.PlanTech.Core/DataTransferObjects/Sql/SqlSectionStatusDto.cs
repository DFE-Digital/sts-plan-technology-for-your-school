using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlSectionStatusDto : SqlDto
{
    public string SectionId { get; set; } = null!;
    public bool Completed { get; set; }
    public bool? HasBeenViewed { get; set; }
    public DateTime? LastCompletionDate { get; set; }
    public string? LastMaturity { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public SubmissionStatus Status { get; set; }
}
