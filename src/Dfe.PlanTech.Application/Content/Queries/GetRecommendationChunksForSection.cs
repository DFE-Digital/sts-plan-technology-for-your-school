using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetRecommendationChunksForSection(IContentRepository repository) // TODO: Make an interface for it
{
    private readonly IContentRepository _repository = repository;

    private async Task<SubTopicRecommendation> GetSubTopicRecommendationFromContentful(Section subtopic, CancellationToken cancellationToken)
    {
        IEnumerable<SubTopicRecommendation> subTopicRecommendations = await _repository.GetEntities<SubTopicRecommendation>(cancellationToken);

        SubTopicRecommendation subTopicRecommendation = subTopicRecommendations.Where(subTopicRecommendation => subTopicRecommendation.Subtopic.Sys.Id.Equals(subtopic.Sys.Id)).First();

        return subTopicRecommendation;
    }
}