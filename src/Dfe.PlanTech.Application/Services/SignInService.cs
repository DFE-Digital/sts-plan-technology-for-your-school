using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class SignInService(
    IEstablishmentWorkflow establishmentWorkflow,
    ISignInWorkflow signInWorkflow,
    IUserWorkflow userWorkflow
) : ISignInService
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly ISignInWorkflow _signInWorkflow =
        signInWorkflow ?? throw new ArgumentNullException(nameof(signInWorkflow));
    private readonly IUserWorkflow _userWorkflow =
        userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public virtual async Task<SqlSignInDto> RecordSignInAsync(
        string dfeSignInRef,
        EstablishmentModel dsiOrganisation
    )
    {
        var user = await _userWorkflow.GetOrCreateUserAsync(dfeSignInRef);
        var establishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
            dsiOrganisation
        );
        var signIn = await _signInWorkflow.CreateSignInAsync(user.Id, establishment?.Id);

        return signIn;
    }

    public async Task<SqlSignInDto> RecordSignInUserOnlyAsync(string dfeSignInRef)
    {
        var user = await _userWorkflow.GetOrCreateUserAsync(dfeSignInRef);
        var signIn = await _signInWorkflow.CreateSignInAsync(user.Id);

        return signIn;
    }
}
