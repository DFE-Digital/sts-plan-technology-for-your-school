using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class SqlEstablishmentDtoBuilder
{
    private static readonly DateTime DefaultDateCreated = new(
        2025,
        1,
        15,
        9,
        0,
        0,
        DateTimeKind.Utc
    );

    private int _id = 1;
    private string _establishmentRef = "100001";
    private string? _groupUid = "group-uid";
    private string? _orgName = "Springfield Primary";

    public SqlEstablishmentDtoBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public SqlEstablishmentDtoBuilder WithEstablishmentRef(string establishmentRef)
    {
        _establishmentRef = establishmentRef;
        return this;
    }

    public SqlEstablishmentDtoBuilder WithGroupUid(string? groupUid)
    {
        _groupUid = groupUid;
        return this;
    }

    public SqlEstablishmentDtoBuilder WithOrgName(string? orgName)
    {
        _orgName = orgName;
        return this;
    }

    public SqlEstablishmentDto Build() =>
        new()
        {
            Id = _id,
            EstablishmentRef = _establishmentRef,
            GroupUid = _groupUid,
            OrgName = _orgName,
            DateCreated = DefaultDateCreated,
        };
}
