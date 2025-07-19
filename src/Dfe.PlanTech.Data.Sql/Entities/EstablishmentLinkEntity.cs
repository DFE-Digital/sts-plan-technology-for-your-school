using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class EstablishmentLinkEntity
{
    public int Id { get; set; }

    public string GroupUid { get; set; } = null!;

    public string EstablishmentName { get; set; } = null!;

    public string Urn { get; set; } = null!;

    public SqlEstablishmentLinkDto AsDto()
    {
        return new SqlEstablishmentLinkDto
        {
            Id = Id,
            GroupUid = GroupUid,
            EstablishmentName = EstablishmentName,
            Urn = Urn,
        };
    }
}
