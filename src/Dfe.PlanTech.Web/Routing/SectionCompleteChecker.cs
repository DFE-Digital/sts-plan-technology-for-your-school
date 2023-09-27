using Dfe.PlanTech.Web.Middleware;

namespace Dfe.PlanTech.Web.Routing;

public static class SectionCompleteChecker
{
  public static readonly UserJourneyStatusChecker SectionComplete = new()
  {
    IsMatchingUserJourney = (userJourneyRouter) => userJourneyRouter?.SectionStatus?.Completed == true,
    ProcessUserJourneyRouter = (userJourneyRouter, cancellationToken) =>
    {
      userJourneyRouter.Status = JourneyStatus.Completed;
      userJourneyRouter.NextQuestion = userJourneyRouter.Section!.Questions.First();
      return Task.CompletedTask;
    }
  };
}

