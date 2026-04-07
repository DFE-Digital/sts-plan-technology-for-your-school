using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class SectionStatusEntity
{
    public string SectionId { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }

    public SubmissionStatus Status { get; set; }

    public DateTime? LastCompletionDate { get; set; }

    public SqlSectionStatusDto AsDto()
    {
        return new SqlSectionStatusDto
        {
            SectionId = SectionId,
            DateCreated = DateCreated,
            DateUpdated = DateUpdated,
            Status = Status,
            LastCompletionDate = LastCompletionDate,
        };
    }
}
