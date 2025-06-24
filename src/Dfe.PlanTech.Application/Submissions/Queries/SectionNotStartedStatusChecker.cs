using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Application.Submissions.Queries;

/// <summary>
/// User journey status checker for when the section for the establishment hasn't been started
/// </summary>
public static class SectionNotStartedStatusChecker
{
    public static readonly ISubmissionStatusChecker SectionNotStarted = new SubmissionStatusChecker()
    {
        IsMatchingSubmissionStatusFunc = (userJourneyRouter) => userJourneyRouter.SectionStatus != null &&
                                                       !userJourneyRouter.SectionStatus.Completed &&
                                                       userJourneyRouter.SectionStatus.Status == Status.NotStarted,
        ProcessSubmissionFunc = (userJourneyRouter, cancellationToken) =>
        {
            userJourneyRouter.Status = Status.NotStarted;
            userJourneyRouter.NextQuestion = userJourneyRouter.Section.Questions[0];
            return Task.CompletedTask;
        }
    };
}
