using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class SignInEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? EstablishmentId { get; set; }

    public DateTime SignInDateTime { get; set; }

    public UserEntity User { get; set; } = default!;

    public SqlSignInDto AsDto()
    {
        return new SqlSignInDto
        {
            Id = Id,
            UserId = UserId,
            EstablishmentId = EstablishmentId,
            SignInDateTime = SignInDateTime,
            User = User.AsDto()
        };
    }
}
