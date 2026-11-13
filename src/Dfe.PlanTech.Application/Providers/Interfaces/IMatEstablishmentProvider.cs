namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface IMatEstablishmentProvider
{
    bool IsBulkAssessment();
    IReadOnlyList<int> GetSelectedEstablishmentIdsFromSession();

    IReadOnlyList<string> GetSelectedSchoolNamesAsync(ICurrentUserProvider currentUser);
}
