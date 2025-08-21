namespace Dfe.PlanTech.Infrastructure.SignIn.Models;

public record UserAuthorisationStatus(bool IsAuthenticated, bool HasOrganisation)
{
    public bool IsAuthorised => HasOrganisation;
}
