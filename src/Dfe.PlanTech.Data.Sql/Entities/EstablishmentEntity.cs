using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("establishment")]
public class EstablishmentEntity
{
    private string _establishmentRef = null!;
    private string? _establishmentType;
    private string _orgName = null!;

    public int Id { get; set; }

    public string? EstablishmentRef { get; set; }

    public string? EstablishmentType { get; set; }

    public string? OrgName { get; set; }

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
