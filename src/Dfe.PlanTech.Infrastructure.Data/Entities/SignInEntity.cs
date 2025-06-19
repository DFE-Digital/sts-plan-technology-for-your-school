using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Infrastructure.Data.Entities;

public class SignInEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? EstablishmentId { get; set; }

    public DateTime SignInDateTime { get; set; }

    public UserEntity User { get; set; } = default!;

    public SignInDto ToDto()
    {
        return new SignInDto
        {
            Id = Id,
            UserId = UserId,
            EstablishmentId = EstablishmentId,
            SignInDateTime = SignInDateTime,
            User = User.ToDto()
        };
    }
}
