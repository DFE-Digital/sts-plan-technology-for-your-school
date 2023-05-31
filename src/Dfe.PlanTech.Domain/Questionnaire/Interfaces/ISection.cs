using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

/// <summary>
/// A subsection of a <see chref="ICategory"/>
/// </summary>
public interface ISection : IContentComponent
{
    public string Name { get; init; }
}
