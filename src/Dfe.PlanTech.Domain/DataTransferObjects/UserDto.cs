namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class UserDto
{
    public int Id { get; set; }

    public string DfeSignInRef { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public List<SignInDto> SignIns { get; set; }

    public List<ResponseDto> Responses { get; set; }
}
