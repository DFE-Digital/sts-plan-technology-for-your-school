namespace Dfe.PlanTech.Infrastructure.SignIns.Models;

public record UserAuthorisationResult(bool PageRequiresAuthorisation, UserAuthorisationStatus UserAuthorisationStatus)
{
    public const string HttpContextKey = "UserAuthorisationResult";

    public bool CanViewPage => !PageRequiresAuthorisation || UserAuthorisationStatus.IsAuthorised;
    public bool AuthenticationMatches => !PageRequiresAuthorisation || UserAuthorisationStatus.IsAuthenticated;
}
