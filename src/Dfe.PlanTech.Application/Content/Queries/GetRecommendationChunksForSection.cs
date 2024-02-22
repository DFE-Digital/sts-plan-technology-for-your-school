using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetRecommendationChunksForSection(IContentRepository repository) // TODO: Make an interface for it
{
    private readonly IContentRepository _repository = repository;

    public async Task<RecommendationIntro> GetRecommendationIntroForSubtopic(Section subTopic, Maturity maturity, CancellationToken cancellationToken)
        => (await GetSubTopicRecommendationFromContentful(subTopic, cancellationToken)).Intros.Where(intro => intro.Maturity.Equals(maturity)).First();

    private async Task<SubTopicRecommendation> GetSubTopicRecommendationFromContentful(Section subTopic, CancellationToken cancellationToken)
    {
        IEnumerable<SubTopicRecommendation> subTopicRecommendations = await _repository.GetEntities<SubTopicRecommendation>(cancellationToken);

        SubTopicRecommendation subTopicRecommendation = subTopicRecommendations.Where(subTopicRecommendation => subTopicRecommendation.Subtopic.Sys.Id.Equals(subTopic.Sys.Id)).First();

        return subTopicRecommendation;
    }
}