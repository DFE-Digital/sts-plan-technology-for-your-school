using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Tests.Shared.Builders.Models;

public sealed class EstablishmentModelBuilder
{
    private string _categoryId = "001";
    private string _categoryName = "School";
    private string _groupUid = "group-uid";
    private string _name = "Springfield Academy Trust";
    private string _uid = "trust-uid";
    private string? _urn;

    public EstablishmentModelBuilder WithCategory(string id, string name)
    {
        _categoryId = id;
        _categoryName = name;
        return this;
    }

    public EstablishmentModelBuilder WithGroupUid(string groupUid)
    {
        _groupUid = groupUid;
        return this;
    }

    public EstablishmentModelBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EstablishmentModelBuilder WithUid(string uid)
    {
        _uid = uid;
        return this;
    }

    public EstablishmentModelBuilder WithUrn(string? urn)
    {
        _urn = urn;
        return this;
    }

    public EstablishmentModel Build() =>
        new()
        {
            Category = new IdWithNameModel { Id = _categoryId, Name = _categoryName },
            GroupUid = _groupUid,
            Name = _name,
            Uid = _uid,
            Urn = _urn,
        };
}
