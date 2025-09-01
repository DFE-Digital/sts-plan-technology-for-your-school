using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.Context.Interfaces
{
    public interface ICurrentUser
    {
        int? UserId { get; }
        string? DsiReference { get; }
        int? EstablishmentId { get; }
        void SetGroupSelectedSchool(string selectedSchoolUrn);
        EstablishmentModel GetEstablishmentModel();
        string? GroupSelectedSchoolUrn { get; }
        bool IsMat { get; }
        bool IsAuthenticated { get; }
        OrganisationModel? Organisation { get; }
    }
}
