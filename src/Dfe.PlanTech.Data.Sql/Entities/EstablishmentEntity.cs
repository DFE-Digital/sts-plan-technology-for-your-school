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
        set => _establishmentRef = TrimToMax(value, EstablishmentRefMaxLengthInclusive);
    }

    [StringLength(EstablishmentTypeMaxLengthInclusive)]
    public string? EstablishmentType
    {
        get => _establishmentType;
        set => _establishmentType = TrimToMaxOrNull(value, EstablishmentTypeMaxLengthInclusive);
    }

    [StringLength(OrgNameMaxLengthInclusive)]
    public string OrgName
    {
        get => _orgName;
        set => _orgName = TrimToMax(value, OrgNameMaxLengthInclusive);
    }

    public string? GroupUid { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    private static string TrimToMax(string value, int maxLengthInclusive)
    {
        return value.Length <= maxLengthInclusive
            ? value
            : value.AsSpan(0, maxLengthInclusive).ToString();
    }

    private static string? TrimToMaxOrNull(string? value, int maxLengthInclusive)
    {
        if (value is null)
            return null;

        return value.Length <= maxLengthInclusive
            ? value
            : value.AsSpan(0, maxLengthInclusive).ToString();
    }

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
