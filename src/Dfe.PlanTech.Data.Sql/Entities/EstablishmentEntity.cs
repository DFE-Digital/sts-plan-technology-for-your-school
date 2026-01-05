using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("establishment")]
public class EstablishmentEntity
{
    public int Id { get; set; }

    public string? EstablishmentRef { get; set; }

    public string? EstablishmentType { get; set; }

    public string? OrgName { get; set; }

    public string? GroupUid { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public SqlEstablishmentDto AsDto()
    {
        return new SqlEstablishmentDto
        {
            Id = Id,
            EstablishmentRef = EstablishmentRef,
            EstablishmentType = EstablishmentType,
            OrgName = OrgName,
            GroupUid = GroupUid,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated
        };
    }
}
