using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interface;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Routing;

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
  public Section? Section { get; }
  public SectionStatusNew? SectionStatus { get; }

  Task GetJourneyStatusForSection(string sectionSlug, CancellationToken cancellationToken);
}