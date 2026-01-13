using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface ISignInWorkflow
{
    Task<SqlSignInDto> RecordSignIn(string dfeSignInRef, EstablishmentModel establishmentModel);
    Task<SqlSignInDto> RecordSignInUserOnly(string dfeSignInRef);
}
