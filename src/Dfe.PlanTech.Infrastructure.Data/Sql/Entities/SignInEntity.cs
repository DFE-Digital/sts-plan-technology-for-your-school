using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class SignInEntity : SqlEntity<SqlSignInDto>
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? EstablishmentId { get; set; }

    public DateTime SignInDateTime { get; set; }

    public UserEntity User { get; set; } = default!;

    protected override SqlSignInDto CreateDto()
    {
        return new SqlSignInDto
        {
            Id = Id,
            UserId = UserId,
            EstablishmentId = EstablishmentId,
            SignInDateTime = SignInDateTime,
            User = User.ToDto()
        };
    }
}
