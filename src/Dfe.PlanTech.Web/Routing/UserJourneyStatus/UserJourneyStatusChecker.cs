namespace Dfe.PlanTech.Web.Routing;

public class UserJourneyStatusChecker
{
  public Func<UserJourneyStatusProcessor, bool> IsMatchingUserJourney { get; init; } = null!;
  public Func<UserJourneyStatusProcessor, CancellationToken, Task> ProcessUserJourneyRouter { get; init; } = null!;
}
