using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Entities;

public class EstablishmentLinkEntity
{
    public int Id { get; set; }

    public string GroupUid { get; set; } = null!;

    public string EstablishmentName { get; set; } = null!;

    public string Urn { get; set; } = null!;

    [NotMapped]
    public int? CompletedSectionsCount { get; set; }

    public EstablishmentLinkDto ToDto()
    {
        return new EstablishmentLinkDto
        {
            Id = Id,
            GroupUid = GroupUid,
            EstablishmentName = EstablishmentName,
            Urn = Urn,
            CompletedSectionsCount = CompletedSectionsCount
        };
    }
}
