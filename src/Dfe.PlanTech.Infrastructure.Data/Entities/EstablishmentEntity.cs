using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Entities;

public class EstablishmentEntity
{
    public const int EstablishmentRefLength = 50;
    public const int EstablishmentTypeLength = 50;
    public const int OrgNameLength = 200;

    private string _establishmentRef = null!;
    private string? _establishmentType;
    private string _orgName = null!;

    public int Id { get; set; }

    [StringLength(EstablishmentRefLength)]
    public string EstablishmentRef
    {
        get => _establishmentRef;
        set
        {
            _establishmentRef = value.Length < EstablishmentRefLength
                ? value
                : value.AsSpan(0, EstablishmentRefLength).ToString();
        }
    }

    [StringLength(EstablishmentTypeLength)]
    public string? EstablishmentType
    {
        get => _establishmentType;
        set
        {
            if (value == null)
                _establishmentType = null;
            else
                _establishmentType = value.Length < EstablishmentTypeLength
                    ? value
                    : value.AsSpan(0, EstablishmentTypeLength).ToString();
        }
    }

    [StringLength(OrgNameLength)]
    public string OrgName
    {
        get => _orgName;
        set
        {
            _orgName = value.Length < OrgNameLength
                ? value
                : value.AsSpan(0, OrgNameLength).ToString();
        }
    }

    public string? GroupUid { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public EstablishmentDto ToDto()
    {
        return new EstablishmentDto
        {
            EstablishmentRef = EstablishmentRef,
            EstablishmentType = EstablishmentType,
            OrgName = OrgName,
            GroupUid = GroupUid,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated
        };
    }
}
