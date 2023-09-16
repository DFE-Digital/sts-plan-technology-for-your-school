using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Domain.SignIns.Models;

namespace Dfe.PlanTech.Domain.Users.Models;

public class User
{
    public int Id { get; set; }

    public string DfeSignInRef { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public List<SignIn> SignIns { get; set; } = new();

    public List<Response> Responses { get; set; } = new();
}
