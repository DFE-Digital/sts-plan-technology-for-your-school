using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Providers;

public class MatEstablishmentProvider(
    IHttpContextAccessor httpContextAccessor,
    IEstablishmentService establishmentService
    ) : IMatEstablishmentProvider
{

    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public async Task<MatEstablishmentModel?> PopulateMatSelectedSchools(ICurrentUserProvider currentUser)
    {
        if (!currentUser.IsMat)
        {
            return null;
        }

        var selectedSchoolNames = await GetSelectedSchoolNames();

        var model = new MatEstablishmentModel(
            selectedSchoolNames.Count != 0,
            selectedSchoolNames.Count,
            selectedSchoolNames
        );

        return model;
    }

    public IEnumerable<int> GetSelectedEstablishmentIdsFromSession()
    {
        var selectedEstablishments =
            _httpContextAccessor.HttpContext!.Session.GetValue(
                SessionConstants.SelectedEstablishmentsKey
            );

        return selectedEstablishments as IEnumerable<int> ?? [];
    }

    private async Task<List<string>> GetSelectedSchoolNames()
    {
        var selectedEstablishmentIds = GetSelectedEstablishmentIdsFromSession().ToArray();

        if (selectedEstablishmentIds.Length == 0)
        {
            return [];
        }

        var schools = new List<string>();

        foreach (var establishmentId in selectedEstablishmentIds)
        {
            var establishment = await establishmentService.GetEstablishmentByIdAsync(establishmentId);

            if (!string.IsNullOrWhiteSpace(establishment.OrgName))
            {
                schools.Add(establishment.OrgName);
            }
        }

        return schools;
    }


}
