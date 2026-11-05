using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface ISignInService
    {
        Task<SqlSignInDto> RecordSignInAsync(
            string dfeSignInRef,
            EstablishmentModel dsiOrganisation
        );
        Task<SqlSignInDto> RecordSignInUserOnly(string dfeSignInRef);
    }
}
