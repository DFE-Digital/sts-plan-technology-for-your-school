using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class SqlGiasEstablishmentDtoBuilder
{
    private static readonly DateTime DefaultSyncedAt = new(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc);

    private int _urn = 100001;
    private string _establishmentName = "Springfield Primary";
    private string? _typeOfEstablishmentName = "Academy converter";
    private string? _ukprn = "10000001";

    public SqlGiasEstablishmentDtoBuilder WithUrn(int urn)
    {
        _urn = urn;
        return this;
    }

    public SqlGiasEstablishmentDtoBuilder WithEstablishmentName(string establishmentName)
    {
        _establishmentName = establishmentName;
        return this;
    }

    public SqlGiasEstablishmentDtoBuilder WithTypeOfEstablishmentName(
        string? typeOfEstablishmentName
    )
    {
        _typeOfEstablishmentName = typeOfEstablishmentName;
        return this;
    }

    public SqlGiasEstablishmentDtoBuilder WithUkprn(string? ukprn)
    {
        _ukprn = ukprn;
        return this;
    }

    public SqlGiasEstablishmentDto Build() =>
        new()
        {
            Urn = _urn,
            EstablishmentName = _establishmentName,
            EstablishmentStatusCode = 1,
            LocalAuthorityCode = 201,
            PhaseCode = 2,
            TypeOfEstablishmentName = _typeOfEstablishmentName,
            Ukprn = _ukprn,
            SyncedAt = DefaultSyncedAt,
        };
}
