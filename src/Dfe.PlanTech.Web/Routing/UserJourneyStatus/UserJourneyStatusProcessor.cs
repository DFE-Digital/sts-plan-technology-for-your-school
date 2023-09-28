using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Exceptions;

namespace Dfe.PlanTech.Web.Routing;

public class UserJourneyStatusProcessor : IUserJourneyStatusProcessor
{
  private readonly IGetSectionQuery _getSectionQuery;
  private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;

  private readonly UserJourneyStatusChecker[] _statusCheckers;

  public UserJourneyStatusProcessor(IGetSectionQuery getSectionQuery,
                           IGetSubmissionStatusesQuery getSubmissionStatusesQuery,
                           IEnumerable<UserJourneyStatusChecker> statusCheckers,
                           IGetLatestResponsesQuery getResponsesQuery,
                           IUser user)
  {
    _getSectionQuery = getSectionQuery;
    _getSubmissionStatusesQuery = getSubmissionStatusesQuery;
    _statusCheckers = statusCheckers.ToArray();

    if (_statusCheckers.Length == 0)
    {
      throw new ArgumentNullException(nameof(statusCheckers));
    }

    GetResponsesQuery = getResponsesQuery;
    User = user;
  }

  public IGetLatestResponsesQuery GetResponsesQuery { get; init; }
  public IUser User { get; init; }

  public JourneyStatus Status { get; set; }
  public Question? NextQuestion { get; set; }

  public Section? Section { get; private set; }
  public SectionStatusNew? SectionStatus { get; private set; }

/// <summary>
/// Get's the current status for the current user's establishment and the given section
/// </summary>
/// <param name="sectionSlug"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
/// <exception cref="ContentfulDataUnavailableException"></exception>
  public async Task GetJourneyStatusForSection(string sectionSlug, CancellationToken cancellationToken)
  {
    var establishmentId = await User.GetEstablishmentId();

    Section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                    throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

    SectionStatus = await _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(establishmentId,
                                                                                      Section,
                                                                                      cancellationToken);

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
