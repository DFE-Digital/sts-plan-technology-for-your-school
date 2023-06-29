using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

/// <summary>
/// A subsection of a <see chref="ICategory"/>
/// </summary>
public interface ISection : IContentComponent
{
    public string Name { get; }

    public Question[] Questions { get; }

    public string FirstQuestionId { get; }

    public Page InterstitialPage { get; }
}
