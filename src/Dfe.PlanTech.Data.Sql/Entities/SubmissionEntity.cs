using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("submission")]
public class SubmissionEntity : IUserActionEntity
{
    public int Id { get; set; }

    public int EstablishmentId { get; set; }

    public EstablishmentEntity Establishment { get; set; } = null!;

    public required string SectionId { get; set; }

    public required string SectionName { get; set; }

    public string? Maturity { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateLastUpdated { get; set; }

    public DateTime? DateCompleted { get; set; }

    public ICollection<ResponseEntity> Responses { get; set; } = [];

    public bool Deleted { get; set; }

    public SubmissionStatus Status { get; set; }

    public Guid? UserActionId { get; set; }

    public SqlSubmissionDto AsDto()
    {
        return new SqlSubmissionDto
        {
            Id = Id,
            EstablishmentId = EstablishmentId,
            Establishment = Establishment.AsDto(),
            SectionId = SectionId,
            SectionName = SectionName,
            Maturity = Maturity,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated,
            DateCompleted = DateCompleted,
            Responses = Responses.Select(r => r.AsDto()).ToList(),
            Deleted = Deleted,
            Status = Status,
        };
    }
}
