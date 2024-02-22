using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

public interface IGetRecommendationIntro
{
    public Task<RecommendationIntro> GetRecommendationIntroForSubtopic(Section subTopic, Maturity maturity, CancellationToken cancellationToken = default);
}