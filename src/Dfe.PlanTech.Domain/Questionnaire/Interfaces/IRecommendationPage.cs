using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationPage : IContentComponent
{
    public string InternalName { get; }

    public string DisplayName { get; }

    public Maturity Maturity { get; }

    public Page Page { get; }
}
