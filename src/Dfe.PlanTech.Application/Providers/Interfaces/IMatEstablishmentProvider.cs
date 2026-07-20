using Dfe.PlanTech.Application.Providers.Interfaces;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface IMatEstablishmentProvider
{
    IReadOnlyList<int> GetSelectedEstablishmentIdsFromSession();

    Task<IReadOnlyList<string>> GetSelectedSchoolNamesAsync(
        ICurrentUserProvider currentUser
    );
}
