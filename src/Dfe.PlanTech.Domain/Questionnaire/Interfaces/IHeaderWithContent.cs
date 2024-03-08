using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IHeaderWithContent
{
  public string Title { get; }

  public string SlugifiedTitle { get; }

  public Header Header { get; }

  public List<ContentComponent> Content { get; }
}