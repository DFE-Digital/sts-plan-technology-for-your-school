using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// A sub-section of a <see chref="Category"/>
/// </summary>
public class Section : ContentComponent, ISection
{
    public string Name { get; init; } = null!;
}
