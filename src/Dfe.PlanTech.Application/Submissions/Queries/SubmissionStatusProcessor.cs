using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
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
    private ISectionComponent? _sectionComponent;

    public IGetLatestResponsesQuery GetResponsesQuery { get; init; }
    public ISectionComponent Section
    {
        get => _sectionComponent ?? throw new ApplicationException("Section is null but it should not be");
        private set => _sectionComponent = value;
    }
    public IUser User { get; init; }
    public Question? NextQuestion { get; set; }
    public SectionStatusNew? SectionStatus { get; private set; }
    public SubmissionStatus Status { get; set; }

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

        var matchingStatusChecker = _statusCheckers.FirstOrDefault(statusChecker => statusChecker.IsMatchingSubmissionStatus(this)) ??
                                    throw new InvalidDataException($"Could not find appropriate status checker for section status {SectionStatus}");

        await matchingStatusChecker.ProcessSubmission(this, cancellationToken);
    }
}
