using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

// TODO: Make Interface
public class GetRecommendationIntro(GetSubTopicRecommendationFromContentfulQuery getSubTopicRecommendationFromContentfulQuery)
{
    private readonly GetSubTopicRecommendationFromContentfulQuery _getSubTopicRecommendationFromContentfulQuery = getSubTopicRecommendationFromContentfulQuery;

    public async Task<RecommendationIntro> GetRecommendationIntroForSubtopic(Section subTopic, Maturity maturity, CancellationToken cancellationToken)
        => (await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(subTopic, cancellationToken)).Intros.Where(intro => intro.Maturity.Equals(maturity)).First();
}