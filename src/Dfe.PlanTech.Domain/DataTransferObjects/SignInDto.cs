namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class SignInDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? EstablishmentId { get; set; }

    public DateTime SignInDateTime { get; set; }

    public UserDto User { get; set; } = default!;
}
