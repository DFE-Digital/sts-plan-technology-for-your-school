using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Middleware;

namespace Dfe.PlanTech.Web.Routing;

public static class SectionNotStartedChecker
{
  public static readonly UserJourneyStatusChecker SectionNotStarted = new()
  {
    IsMatchingUserJourney = (userJourneyRouter) => userJourneyRouter.SectionStatus != null && 
                                                  !userJourneyRouter.SectionStatus.Completed &&
                                                  userJourneyRouter.SectionStatus.Status == Status.NotStarted,
    ProcessUserJourneyRouter = (userJourneyRouter, cancellationToken) =>
    {
      userJourneyRouter.Status = JourneyStatus.NotStarted;
      userJourneyRouter.NextQuestion = userJourneyRouter.Section!.Questions.First();
      return Task.CompletedTask;
    }
  };
}