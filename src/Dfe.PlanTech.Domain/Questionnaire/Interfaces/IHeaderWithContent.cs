using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IHeaderWithContent
{
  public Header Header { get; }
  public List<ContentComponent> Content { get; }
}