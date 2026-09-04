using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class GiasEstablishmentEntity
{
    [Required]
    public int Urn { get; init; }

    public Int64? Uprn { get; init; } = null!;

    public int? EstablishmentNumber { get; init; }

    [Required]
    public string EstablishmentName { get; init; } = null!;

    [Required]
    public int EstablishmentStatusCode { get; init; }

    [Required]
    public int LocalAuthorityCode { get; init; }

    [Required]
    public int PhaseCode { get; init; }

    public int? TypeOfEstablishmentCode { get; init; }

    public GiasTypeOfEstablishmentEntity? TypeOfEstablishment { get; init; }

    [Required]
    public string? Ukprn { get; init; }

    [Required]
    public DateTime SyncedAt { get; init; }

    public GiasGroupMembershipEntity? GroupMembership { get; init; }

    public EstablishmentEntity? DboEstablishment { get; init; }

    public SqlGiasEstablishmentDto AsDto()
    {
        return new SqlGiasEstablishmentDto
        {
            Urn = Urn,
            Uprn = Uprn,
            EstablishmentNumber = EstablishmentNumber,
            EstablishmentName = EstablishmentName,
            EstablishmentStatusCode = EstablishmentStatusCode,
            LocalAuthorityCode = LocalAuthorityCode,
            PhaseCode = PhaseCode,
            SyncedAt = SyncedAt,
            Ukprn = Ukprn,
            TypeOfEstablishmentCode = TypeOfEstablishmentCode,
        };
    }

    public EstablishmentBasicDto AsEstablishmentBasicDto()
    {
        return new EstablishmentBasicDto
        {
            Urn = Urn.ToString(),
            Name = EstablishmentName
        };
    }
}
