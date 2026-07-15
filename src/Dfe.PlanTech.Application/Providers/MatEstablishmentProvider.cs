using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Providers;

public class MatEstablishmentProvider(
    IHttpContextAccessor httpContextAccessor,
    IEstablishmentService establishmentService
) : IMatEstablishmentProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor
        ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private readonly IEstablishmentService _establishmentService =
        establishmentService
        ?? throw new ArgumentNullException(nameof(establishmentService));

    public async Task<MatEstablishmentModel?> PopulateMatSelectedSchools(
        ICurrentUserProvider currentUser
    )
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        if (!currentUser.IsMat)
        {
            return null;
        }

        var selectedSchoolNames =
            await GetSelectedSchoolNames();

        if (
            selectedSchoolNames.Count == 0
            && !string.IsNullOrWhiteSpace(
                currentUser.GroupSelectedSchoolName
            )
        )
        {
            selectedSchoolNames.Add(
                currentUser.GroupSelectedSchoolName
            );
        }

        return new MatEstablishmentModel(
            selectedSchoolNames.Count > 0,
            selectedSchoolNames.Count,
            selectedSchoolNames
        );
    }

    public IEnumerable<int> GetSelectedEstablishmentIdsFromSession()
    {
        var session =
            _httpContextAccessor.HttpContext?.Session;

        if (session is null)
        {
            return [];
        }

        return session.GetValue(
            SessionConstants.SelectedEstablishmentsKey
        ) as IEnumerable<int> ?? [];
    }

    private async Task<List<string>> GetSelectedSchoolNames()
    {
        var selectedEstablishmentIds =
            GetSelectedEstablishmentIdsFromSession()
                .Distinct()
                .ToArray();

        if (selectedEstablishmentIds.Length == 0)
        {
            return [];
        }

        var schoolNames = new List<string>();

        foreach (var establishmentId in selectedEstablishmentIds)
        {
            var establishment =
                await _establishmentService
                    .GetEstablishmentByIdAsync(establishmentId);

            if (
                establishment is not null
                && !string.IsNullOrWhiteSpace(
                    establishment.OrgName
                )
            )
            {
                schoolNames.Add(establishment.OrgName);
            }
        }

        return schoolNames;
    }
}
