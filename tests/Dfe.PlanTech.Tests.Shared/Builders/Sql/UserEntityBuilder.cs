using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class UserEntityBuilder
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

    private string _dfeSignInRef = "dsi-user-reference";

    public UserEntityBuilder WithDfeSignInRef(string dfeSignInRef)
    {
        _dfeSignInRef = dfeSignInRef;
        return this;
    }

    public UserEntity Build() =>
        new() { DfeSignInRef = _dfeSignInRef, DateCreated = DefaultDateCreated };
}
