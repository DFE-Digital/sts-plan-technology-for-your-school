namespace Dfe.PlanTech.Core.Models;

public record UserAuthorisationStatus(bool IsAuthenticated, bool HasOrganisation)
{
    public bool IsAuthorised => HasOrganisation;
}
