using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("submission")]
public class SubmissionEntity
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

    public SqlSubmissionDto AsDto()
    {
        return new SqlSubmissionDto
        {
            Id = Id,
            EstablishmentId = EstablishmentId,
            Establishment = Establishment.AsDto(),
            Completed = Completed,
            SectionId = SectionId,
            SectionName = SectionName,
            Maturity = Maturity,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated,
            DateCompleted = DateCompleted,
            Responses = Responses.Select(r => r.AsDto()),
            Deleted = Deleted,
            Viewed = Viewed,
            Status = Status
        };
    }
}
