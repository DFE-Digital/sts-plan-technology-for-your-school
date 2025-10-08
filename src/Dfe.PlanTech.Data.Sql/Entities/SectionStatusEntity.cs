using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class SectionStatusEntity
{
    public string SectionId { get; set; } = null!;

    public bool Completed { get; set; }

    public string? LastMaturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }

    public bool? Viewed { get; set; }

    public DateTime? LastCompletionDate { get; set; }

    public SqlSectionStatusDto AsDto()
    {
        return new SqlSectionStatusDto
        {
            SectionId = SectionId,
            Completed = Completed,
            LastMaturity = LastMaturity,
            DateCreated = DateCreated,
            DateUpdated = DateUpdated,
            HasBeenViewed = Viewed,
            LastCompletionDate = LastCompletionDate,
        };
    }
}
