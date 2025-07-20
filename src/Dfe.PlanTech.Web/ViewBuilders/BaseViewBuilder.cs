using System.Security.Authentication;
using Dfe.PlanTech.Web.Context;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class BaseViewBuilder(
    CurrentUser currentUser
)
{
    protected CurrentUser CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    protected int GetUserIdOrThrowException()
    {
        return CurrentUser.UserId ?? throw new AuthenticationException("User is not authenticated");
    }

    protected int GetEstablishmentIdOrThrowException()
    {
        return CurrentUser.EstablishmentId ?? throw new InvalidDataException(nameof(currentUser.EstablishmentId));
    }

    protected string GetDsiReferenceOrThrowException()
    {
        return CurrentUser.DsiReference ?? throw new AuthenticationException("User is not authenticated");
    }
}
