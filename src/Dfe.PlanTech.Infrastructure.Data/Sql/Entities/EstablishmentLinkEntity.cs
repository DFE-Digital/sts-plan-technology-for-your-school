using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class EstablishmentLinkEntity : SqlEntity<SqlEstablishmentLinkDto>
{
    public int Id { get; set; }

    public string GroupUid { get; set; } = null!;

    public string EstablishmentName { get; set; } = null!;

    public string Urn { get; set; } = null!;

    [NotMapped]
    public int? CompletedSectionsCount { get; set; }

    protected override SqlEstablishmentLinkDto CreateDto()
    {
        return new SqlEstablishmentLinkDto
        {
            Id = Id,
            GroupUid = GroupUid,
            EstablishmentName = EstablishmentName,
            Urn = Urn,
            CompletedSectionsCount = CompletedSectionsCount
        };
    }
}
