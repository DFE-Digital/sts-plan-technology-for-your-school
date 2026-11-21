using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class GiasEstablishmentEntityBuilder
{
    private static readonly DateTime DefaultSyncedAt = new(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc);

    private int _urn = 100001;
    private string _establishmentName = "Springfield Primary";
    private GiasTypeOfEstablishmentEntity? _typeOfEstablishment;

    public GiasEstablishmentEntityBuilder WithUrn(int urn)
    {
        _urn = urn;
        return this;
    }

    public GiasEstablishmentEntityBuilder WithEstablishmentName(string establishmentName)
    {
        _establishmentName = establishmentName;
        return this;
    }

    public GiasEstablishmentEntityBuilder WithTypeOfEstablishment(
        int typeOfEstablishmentCode,
        string typeOfEstablishmentName
    )
    {
        _typeOfEstablishment = new GiasTypeOfEstablishmentEntity
        {
            TypeOfEstablishmentCode = typeOfEstablishmentCode,
            TypeOfEstablishmentName = typeOfEstablishmentName,
        };
        return this;
    }

    public GiasEstablishmentEntity Build() =>
        new()
        {
            Urn = _urn,
            EstablishmentName = _establishmentName,
            EstablishmentStatusCode = 1,
            LocalAuthorityCode = 201,
            PhaseCode = 2,
            Ukprn = "10000001",
            SyncedAt = DefaultSyncedAt,
            TypeOfEstablishmentCode = _typeOfEstablishment?.TypeOfEstablishmentCode,
            TypeOfEstablishment = _typeOfEstablishment,
        };
}
