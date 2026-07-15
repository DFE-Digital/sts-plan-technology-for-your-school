using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface IMatEstablishmentProvider
{

    public Task<MatEstablishmentModel?> PopulateMatSelectedSchools(ICurrentUserProvider currentUser);
    public IEnumerable<int> GetSelectedEstablishmentIdsFromSession();
}

