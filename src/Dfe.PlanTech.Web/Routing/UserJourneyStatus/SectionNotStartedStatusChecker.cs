using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// User journey status checker for when the section for the establishment hasn't been started
/// </summary>
public static class SectionNotStartedStatusChecker
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