using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class SectionStatusEntity : SqlEntity<SqlSectionStatusDto>
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? LastMaturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }

    public bool? Viewed { get; set; }

    public DateTime? LastCompletionDate { get; set; }

    protected override SqlSectionStatusDto CreateDto()
    {
        return new SqlSectionStatusDto
        {
            SectionId = SectionId,
            Completed = Completed,
            LastMaturity = LastMaturity,
            DateCreated = DateCreated,
            DateUpdated = DateUpdated,
            Viewed = Viewed,
            LastCompletionDate = LastCompletionDate
        };
    }
}
