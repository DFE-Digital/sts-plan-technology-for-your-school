using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class EstablishmentEntityBuilder
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

    private int _id;
    private string _establishmentRef = "100001";
    private string? _establishmentType = "Primary";
    private string? _groupUid = "group-uid";
    private string? _orgName = "Springfield Primary";

    public EstablishmentEntityBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public EstablishmentEntityBuilder WithEstablishmentRef(string establishmentRef)
    {
        _establishmentRef = establishmentRef;
        return this;
    }

    public EstablishmentEntityBuilder WithEstablishmentType(string? establishmentType)
    {
        _establishmentType = establishmentType;
        return this;
    }

    public EstablishmentEntityBuilder WithGroupUid(string? groupUid)
    {
        _groupUid = groupUid;
        return this;
    }

    public EstablishmentEntityBuilder WithOrgName(string? orgName)
    {
        _orgName = orgName;
        return this;
    }

    public EstablishmentEntity Build() =>
        new()
        {
            Id = _id,
            EstablishmentRef = _establishmentRef,
            EstablishmentType = _establishmentType,
            GroupUid = _groupUid,
            OrgName = _orgName,
            DateCreated = DefaultDateCreated,
        };
}
