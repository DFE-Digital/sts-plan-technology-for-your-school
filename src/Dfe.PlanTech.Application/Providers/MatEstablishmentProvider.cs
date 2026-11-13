using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Helpers;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Providers;

public class MatEstablishmentProvider(
    IHttpContextAccessor httpContextAccessor,
    IEstablishmentService establishmentService
) : IMatEstablishmentProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    public bool IsBulkAssessment() => GetSelectedEstablishmentIdsFromSession().Count > 0;

    public IReadOnlyList<int> GetSelectedEstablishmentIdsFromSession()
    {
        return _httpContextAccessor
                .HttpContext?.Session.GetSelectedSchools()
                .Select(e => e.EstablishmentId)
                .Distinct()
                .ToArray()
            ?? [];
    }

    public IReadOnlyList<string> GetSelectedSchoolNamesAsync(ICurrentUserProvider currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        if (!currentUser.IsMat)
        {
            return [];
        }

        var schoolNames = currentUser!.GetSelectedSchoolNames();
        if (schoolNames.Count == 0)
        {
            var school = currentUser.GetFocusedSchool();
            return school is null ? [] : [school.Name];
        }

        return schoolNames;
    }
}
