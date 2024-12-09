using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IRecordUserSignInCommand
{
    /// <summary>
    /// Record a standard user sign in
    /// </summary>
    Task<SignIn> RecordSignIn(RecordUserSignInDto recordUserSignInDto);

    /// <summary>
    /// Record sign in for a user that is missing an organisation, for analytics purposes.
    /// </summary>
    Task<SignIn> RecordSignInUserOnly(string dfeSignInRef);
}
