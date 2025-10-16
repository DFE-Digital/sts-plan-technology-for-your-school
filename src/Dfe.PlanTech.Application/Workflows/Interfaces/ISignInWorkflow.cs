using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface ISignInWorkflow
{
    Task<SqlSignInDto> RecordSignIn(string dfeSignInRef, DsiOrganisationModel? dsiOrganisationModel);
    Task<SqlSignInDto> RecordSignInUserOnly(string dfeSignInRef);
}
