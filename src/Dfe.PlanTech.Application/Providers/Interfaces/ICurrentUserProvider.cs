using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface ICurrentUserProvider
{
    string? DsiReference { get; }
    Guid? SessionId { get; }
    int? UserId { get; }
    public SqlEstablishmentDto? UserEstablishment { get; }

    // User Organisation - the organisation the currently logged in user is linked to (from OIDC claims)
    // For school users, this is the establishmentId for the school
    // For MAT users, this represents the MAT/group, not the selected establishment
    EstablishmentModel? UserOrganisation { get; }

    bool IsAuthenticated { get; }
    bool IsSchool { get; }
    bool IsGroup { get; }
    bool IsMat { get; }
    bool IsSatOrSSat { get; }
    bool CanSelectSchools { get; }

    void SetEstablishment(SqlEstablishmentDto establishment);

    public void ClearFocusedSchool();
    SelectedEstablishmentDataModel? FocusedSchool { get; }
    Task SetFocusedSchoolAsync(string selectedSchoolUrn, string selectedSchoolName);

    IReadOnlyList<SelectedEstablishmentDataModel> GroupSchools { get; }
    Task SetGroupSchools(IEnumerable<SelectedEstablishmentDataModel> schools);

    void ClearSelectedEstablishments();
    IReadOnlyList<int> GetSelectedEstablishmentIds();
    IReadOnlyList<string> GetSelectedEstablishmentNames();
    IReadOnlyList<SelectedEstablishmentDataModel> GetSelectedEstablishments();
    void SetSelectedEstablishments(SelectedEstablishmentDataModel establishment);
    void SetSelectedEstablishments(IList<SelectedEstablishmentDataModel> establishments);
}
