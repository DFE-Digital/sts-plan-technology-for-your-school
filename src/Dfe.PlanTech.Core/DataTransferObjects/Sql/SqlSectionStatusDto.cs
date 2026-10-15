using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

/// <summary>
/// Used to handle data from GetSectionStatusesAsync
/// </summary>
public class SqlSectionStatusDto : ISqlDto
{
    public string SectionId { get; set; } = null!;
    public SubmissionStatus Status { get; set; }
    public DateTime? LastCompletionDate { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public Guid? CreatedUserActionId { get; set; }
    public Guid? LastUpdatedUserActionId { get; set; }
    public Guid? CompletedUserActionId { get; set; }
}
