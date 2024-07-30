using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

/// <summary>
/// Gets current journey status for a user's establishment and a given section,
/// along with bits of needed information for this (e.g. Section)
/// </summary>
public interface ISubmissionStatusProcessor
{
    public IGetLatestResponsesQuery GetResponsesQuery { get; }
    public IUser User { get; }
    public SubmissionStatus Status { get; set; }
    public Question? NextQuestion { get; set; }
    public ISectionComponent Section { get; }
    public SectionStatusNew? SectionStatus { get; }

    Task GetJourneyStatusForSection(string sectionSlug, CancellationToken cancellationToken);
    Task GetJourneyStatusForSectionRecommendation(string sectionSlug, CancellationToken cancellationToken);
}
