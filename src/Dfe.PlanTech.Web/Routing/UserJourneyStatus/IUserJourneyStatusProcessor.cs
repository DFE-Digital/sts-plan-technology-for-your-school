using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Web.Routing;

public interface IUserJourneyStatusProcessor
{
  public IGetLatestResponsesQuery GetResponsesQuery { get; }
  public IUser User { get; }

  public JourneyStatus Status { get; set; }
  public Question? NextQuestion { get; set; }

  public Section? Section { get; }
  public SectionStatusNew? SectionStatus { get; }

  Task GetJourneyStatusForSection(string sectionSlug, CancellationToken cancellationToken);
}