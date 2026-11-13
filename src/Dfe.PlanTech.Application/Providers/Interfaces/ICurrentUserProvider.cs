using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface ICurrentUserProvider
{
    string DsiReference { get; }
    Guid? SessionId { get; }
    int UserId { get; }

    // User Organisation - the organisation the currently logged in user is linked to (from OIDC claims)
    // For school users, this is the establishmentId for the school
    // For MAT users, this represents the MAT/group, not the selected establishment
    EstablishmentModel? DsiOrganisation { get; }

    bool IsAuthenticated { get; }
    bool IsSchool { get; }
    bool IsTrust { get; }
    bool IsMat { get; }
    bool IsSatOrSSat { get; }
    bool CanSelectSchools { get; }

    public SqlEstablishmentDto Establishment { get; }
    void SetEstablishment(SqlEstablishmentDto establishment);

    public void ClearFocusedSchool();
    SelectedEstablishmentDataModel GetFocusedSchool();
    Task SetFocusedSchoolAsync(string selectedSchoolUrn, string selectedSchoolName);

    List<SelectedEstablishmentDataModel> GetGroupSchools();
    Task SetGroupSchools(IEnumerable<SelectedEstablishmentDataModel> schools);

    void ClearSelectedSchools();
    List<int> GetSelectedSchoolEstablishmentIds();
    List<string> GetSelectedSchoolNames();
    List<SelectedEstablishmentDataModel> GetSelectedSchools();
    void SetSelectedSchools(SelectedEstablishmentDataModel establishment);
    void SetSelectedSchools(IEnumerable<SelectedEstablishmentDataModel> establishments);
}
