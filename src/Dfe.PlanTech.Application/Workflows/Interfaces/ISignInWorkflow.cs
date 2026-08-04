using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface ISignInWorkflow
{
    Task<(EstablishmentModel updatedOrganisation, SqlSignInDto signIn)> RecordSignIn(
        string dfeSignInRef,
        EstablishmentModel establishmentModel
    );
    Task<SqlSignInDto> RecordSignInUserOnly(string dfeSignInRef);
}
