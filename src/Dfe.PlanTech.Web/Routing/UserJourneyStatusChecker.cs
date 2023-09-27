namespace Dfe.PlanTech.Web.Routing;

public class UserJourneyStatusChecker
{
  public Func<UserJourneyRouter, bool> IsMatchingUserJourney { get; init; } = null!;
  public Func<UserJourneyRouter, CancellationToken, Task> ProcessUserJourneyRouter { get; init; } = null!;
}
