using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class SubmissionEntity : SqlEntity<SqlSubmissionDto>
{
    public int Id { get; set; }

    public int EstablishmentId { get; set; }

    public EstablishmentEntity Establishment { get; set; } = null!;

    public bool Completed { get; set; }

    public required string SectionId { get; set; }

    public required string SectionName { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateLastUpdated { get; set; }

    public DateTime? DateCompleted { get; set; }

    public IEnumerable<ResponseEntity> Responses { get; set; } = [];

    public bool Deleted { get; set; }

    public bool Viewed { get; set; }

    public string? Status { get; set; }

    protected override SqlSubmissionDto CreateDto()
    {
        return new SqlSubmissionDto
        {
            Id = Id,
            EstablishmentId = EstablishmentId,
            Establishment = Establishment.ToDto(),
            Completed = Completed,
            SectionId = SectionId,
            SectionName = SectionName,
            Maturity = Maturity,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated,
            DateCompleted = DateCompleted,
            Responses = Responses.Select(r => r.ToDto()),
            Deleted = Deleted,
            Viewed = Viewed,
            Status = Status
        };
    }
}
