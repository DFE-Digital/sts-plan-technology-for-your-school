using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class SqlSignInDtoBuilder
{
    private int _id = 1;
    private int _userId = 42;
    private int? _establishmentId = 301;

    public SqlSignInDtoBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public SqlSignInDtoBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public SqlSignInDtoBuilder WithEstablishmentId(int? establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public SqlSignInDto Build() =>
        new()
        {
            Id = _id,
            UserId = _userId,
            EstablishmentId = _establishmentId,
            SignInDateTime = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc),
        };
}
