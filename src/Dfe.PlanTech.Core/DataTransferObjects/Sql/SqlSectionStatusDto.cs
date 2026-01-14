using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

/// <summary>
/// Used to handle data from stored procedure [dbo].[GetSectionStatuses]
/// </summary>
public class SqlSectionStatusDto : ISqlDto
{
    public string SectionId { get; set; } = null!;
    public bool? HasBeenViewed { get; set; }
    public DateTime? LastCompletionDate { get; set; }
    public string? LastMaturity { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public SubmissionStatus Status { get; set; }
}
