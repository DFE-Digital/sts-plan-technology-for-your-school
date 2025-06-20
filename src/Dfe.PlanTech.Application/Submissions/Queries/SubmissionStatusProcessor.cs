using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Web.Routing;

public class SubmissionStatusProcessor : ISubmissionStatusProcessor
{
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
    private readonly ISubmissionStatusChecker[] _statusCheckers;
    private ISectionComponent? _sectionComponent = null; // Initialize the field with null

    public IGetLatestResponsesQuery GetResponsesQuery { get; init; }
    public ISectionComponent Section
    {
        get => _sectionComponent ?? throw new ContentfulDataUnavailableException("Section is null but it should not be");
        private set => _sectionComponent = value;
    }
    public IUser User { get; init; }
    public Question? NextQuestion { get; set; }
    public SectionStatus? SectionStatus { get; private set; }
    public Status Status { get; set; }

    public SubmissionStatusProcessor(IGetSectionQuery getSectionQuery,
                                    IGetSubmissionStatusesQuery getSubmissionStatusesQuery,
                                    IEnumerable<ISubmissionStatusChecker> statusCheckers,
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

    public async Task GetJourneyStatusForSection(string sectionSlug, bool completed, CancellationToken cancellationToken)
    {
        await GetJourneyStatus(sectionSlug, completed, cancellationToken);
    }

    public async Task GetJourneyStatusForSectionRecommendation(string sectionSlug, bool completed, CancellationToken cancellationToken)
    {
        await GetJourneyStatus(sectionSlug, completed, cancellationToken);
    }

    /// <summary>
    /// Get's the current status or most recently completed status for the current user's establishment and the given section
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="complete"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ContentfulDataUnavailableException"></exception>
    private async Task GetJourneyStatus(string sectionSlug, bool complete, CancellationToken cancellationToken)
    {
        var establishmentId = await User.GetEstablishmentId();

        Section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        SectionStatus = await _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(establishmentId,
                                                                                          Section,
                                                                                          complete,
                                                                                          cancellationToken);

        var matchingStatusChecker = Array.Find(_statusCheckers, statusChecker => statusChecker.IsMatchingSubmissionStatus(this)) ??
                                    throw new InvalidDataException($"Could not find appropriate status checker for section status {SectionStatus}");

        await matchingStatusChecker.ProcessSubmission(this, cancellationToken);
    }
}
