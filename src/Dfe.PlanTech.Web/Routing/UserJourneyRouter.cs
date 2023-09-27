using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Exceptions;
using Dfe.PlanTech.Web.Middleware;

namespace Dfe.PlanTech.Web.Routing;

public class UserJourneyRouter
{
  private readonly IGetSectionQuery _getSectionQuery;
  private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;

  private readonly UserJourneyStatusChecker[] _statusCheckers = new[] {
    SectionNotStartedChecker.SectionNotStarted,
    SectionCompleteChecker.SectionComplete,
    CheckAnswersOrNextQuestionChecker.CheckAnswersOrNextQuestion
  };

  public UserJourneyRouter(IGetSectionQuery getSectionQuery,
                           IGetSubmissionStatusesQuery getSubmissionStatusesQuery,
                           IGetLatestResponsesQuery getResponsesQuery,
                           IUser user)
  {
    _getSectionQuery = getSectionQuery;
    _getSubmissionStatusesQuery = getSubmissionStatusesQuery;

    GetResponsesQuery = getResponsesQuery;
    User = user;
  }

  public readonly IGetLatestResponsesQuery GetResponsesQuery;
  public readonly IUser User;

  public JourneyStatus Status { get; set; }
  public Question? NextQuestion { get; set; }

  public Section? Section { get; private set; }
  public SectionStatusNew? SectionStatus { get; private set; }

  public async Task GetJourneyStatusForSection(string sectionSlug, CancellationToken cancellationToken)
  {
    var establishmentId = await User.GetEstablishmentId();

    Section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                    throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

    var sectionStatus = await _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(establishmentId,
                                                                                          Section,
                                                                                          cancellationToken);
    if (sectionStatus != null)
    {
      SectionStatus = sectionStatus.SectionStatus;
    }

    foreach (var statusChecker in _statusCheckers)
    {
      if (statusChecker.IsMatchingUserJourney(this))
      {
        await statusChecker.ProcessUserJourneyRouter(this, cancellationToken);
        return;
      }
    }
  }
}
