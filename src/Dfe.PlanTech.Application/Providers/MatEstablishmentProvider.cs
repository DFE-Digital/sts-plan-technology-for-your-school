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
                .HttpContext?.Session.GetSelectedEstablishments()
                .Select(e => e.Id)
                .Distinct()
                .ToArray()
            ?? [];
    }

    public async Task<IReadOnlyList<string>> GetSelectedSchoolNamesAsync(
        ICurrentUserProvider currentUser
    )
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        if (!currentUser.IsMat)
        {
            return [];
        }

        var selectedEstablishmentIds = GetSelectedEstablishmentIdsFromSession();
        if (selectedEstablishmentIds.Count == 0)
        {
            return string.IsNullOrWhiteSpace(currentUser.FocusedSchool?.Name)
                ? []
                : [currentUser.FocusedSchool.Name];
        }

        var schoolNames = new List<string>();

        foreach (var establishmentId in selectedEstablishmentIds)
        {
            var establishment = await _establishmentService.GetEstablishmentByIdAsync(
                establishmentId
            );

            if (establishment is not null && !string.IsNullOrWhiteSpace(establishment.OrgName))
            {
                schoolNames.Add(establishment.OrgName);
            }
        }

        return schoolNames;
    }
}
