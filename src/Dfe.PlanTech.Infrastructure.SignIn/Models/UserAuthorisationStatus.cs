namespace Dfe.PlanTech.Infrastructure.SignIns.Models;

public record UserAuthorisationStatus(bool IsAuthenticated, bool HasOrganisation)
{
  public bool IsAuthorised => HasOrganisation;
}
