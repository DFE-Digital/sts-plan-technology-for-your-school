using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Infrastructure.Data.Entities;

namespace Dfe.PlanTech.Domain.Users.Models;

public class UserEntity
{
    public int Id { get; set; }

    public string DfeSignInRef { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public List<SignInEntity> SignIns { get; set; } = new();

    public List<ResponseEntity> Responses { get; set; } = new();

    public UserDto ToDto()
    {
        return new UserDto
        {
            Id = Id,
            DfeSignInRef = DfeSignInRef,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated,
            SignIns = SignIns.Select(si => si.ToDto()),
            Responses = Responses.Select(r => r.ToDto())
        }
}
