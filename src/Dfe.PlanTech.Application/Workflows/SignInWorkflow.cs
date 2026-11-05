using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class SignInWorkflow(ISignInRepository signInRepository) : ISignInWorkflow
{
    private readonly ISignInRepository _signInRepository =
        signInRepository ?? throw new ArgumentNullException(nameof(signInRepository));

    public async Task<SqlSignInDto> CreateSignInAsync(int userId, int? establishmentId = null)
    {
        var signIn = await _signInRepository.CreateSignInAsync(userId, establishmentId);
        return signIn.AsDto();
    }
}
