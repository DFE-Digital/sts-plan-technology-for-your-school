namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// User journey status checker for when the section for the establishment is complete
/// </summary>
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

