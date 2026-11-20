using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Tests.Shared.Builders.Models;

public sealed class SelectedEstablishmentDataModelBuilder
{
    private int _establishmentId = 1;
    private string _name = "Springfield Primary";
    private string _urn = "100001";

    public SelectedEstablishmentDataModelBuilder WithEstablishmentId(int establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public SelectedEstablishmentDataModelBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SelectedEstablishmentDataModelBuilder WithUrn(string urn)
    {
        _urn = urn;
        return this;
    }

    public SelectedEstablishmentDataModel Build() =>
        new()
        {
            EstablishmentId = _establishmentId,
            Name = _name,
            Urn = _urn,
        };
}
