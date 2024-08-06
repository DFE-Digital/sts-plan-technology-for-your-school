using Dfe.PlanTech.Domain.SignIns.Models;

namespace Dfe.PlanTech.Domain.Users.Models;

public class RecordUserSignInDto
{
    public required string DfeSignInRef { get; init; }
    public required Organisation Organisation { get; init; }
}
