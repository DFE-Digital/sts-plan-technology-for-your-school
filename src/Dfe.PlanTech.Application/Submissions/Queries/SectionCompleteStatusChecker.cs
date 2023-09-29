using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// User journey status checker for when the section for the establishment is complete
/// </summary>
public static class SectionCompleteStatusChecker
{
  public static readonly ISubmissionStatusChecker SectionComplete = new SubmissionStatusChecker()
  {
    IsMatchingSubmissionStatusFunc = (userJourneyRouter) => userJourneyRouter?.SectionStatus?.Completed == true,
    ProcessSubmissionFunc = (userJourneyRouter, cancellationToken) =>
    {
      userJourneyRouter.Status = SubmissionStatus.Completed;
      userJourneyRouter.NextQuestion = userJourneyRouter.Section!.Questions.First();
      return Task.CompletedTask;
    }
  };
}

