using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("establishment")]
public class EstablishmentEntity
{
    public const int EstablishmentRefMaxLengthInclusive = 50;
    public const int EstablishmentTypeMaxLengthInclusive = 50;
    public const int OrgNameMaxLengthInclusive = 200;

    private string _establishmentRef = null!;
    private string? _establishmentType;
    private string _orgName = null!;

    public int Id { get; set; }

    [StringLength(EstablishmentRefMaxLengthInclusive)]
    public string EstablishmentRef
    {
        get => _establishmentRef;
        set
        {
            _establishmentRef = value.Length <= EstablishmentRefMaxLengthInclusive
                ? value
                : value.AsSpan(0, EstablishmentRefMaxLengthInclusive).ToString();
        }
    }

    [StringLength(EstablishmentTypeMaxLengthInclusive)]
    public string? EstablishmentType
    {
        get => _establishmentType;
        set
        {
            if (value == null)
                _establishmentType = null;
            else
                _establishmentType = value.Length <= EstablishmentTypeMaxLengthInclusive
                    ? value
                    : value.AsSpan(0, EstablishmentTypeMaxLengthInclusive).ToString();
        }
    }

    [StringLength(OrgNameMaxLengthInclusive)]
    public string OrgName
    {
        get => _orgName;
        set
        {
            _orgName = value.Length <= OrgNameMaxLengthInclusive
                ? value
                : value.AsSpan(0, OrgNameMaxLengthInclusive).ToString();
        }
    }

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
