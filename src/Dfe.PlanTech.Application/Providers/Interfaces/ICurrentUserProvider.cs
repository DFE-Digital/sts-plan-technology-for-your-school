using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface ICurrentUserProvider
{
    string DsiReference { get; }
    int? FocusedEstablishmentId { get; }
    Guid? SessionId { get; }
    int? TrustEstablishmentId { get; }
    int UserEstablishmentId { get; }
    int UserId { get; }

    // User Organisation - the organisation the currently logged in user is linked to (from OIDC claims)
    // For school users, this is the establishmentId for the school
    // For MAT users, this represents the MAT/group, not the selected establishment
    EstablishmentModel? DsiOrganisation { get; }

    bool IsAuthenticated { get; }
    bool IsSchool { get; }
    bool IsMat { get; }
    bool IsSatOrSSat { get; }
    bool IsTrust { get; }
    bool CanSelectSchools { get; }

    public void ClearFocusedEstablishment();
    Task<SelectedEstablishmentDataModel?> GetFocusedEstablishmentAsync();
    Task SetFocusedEstablishmentAsync(string urn, string name);

    Task<List<SelectedEstablishmentDataModel>> GetGroupSchoolsAsync();
    Task SetGroupSchoolsAsync(IEnumerable<SelectedEstablishmentDataModel> schools);

    void ClearSelectedSchools();
    List<int>? GetSelectedSchoolEstablishmentIds();
    List<string>? GetSelectedSchoolNames();
    List<SelectedEstablishmentDataModel>? GetSelectedSchools();
    Task SetSelectedSchoolsAsync(SelectedEstablishmentDataModel establishment);
    Task SetSelectedSchoolsAsync(IEnumerable<SelectedEstablishmentDataModel> establishments);

    public Task<SqlEstablishmentDto> GetUserEstablishmentAsync();
    void SetUserEstablishment(SqlEstablishmentDto establishment);
}
