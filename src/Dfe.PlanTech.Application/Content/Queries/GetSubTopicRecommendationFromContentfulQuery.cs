using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

// TODO: Make Interface
// TODO: Make into a service
public class GetSubTopicRecommendationFromContentfulQuery(IContentRepository repository)
{
    private readonly IContentRepository _repository = repository;

    public async Task<SubTopicRecommendation> GetSubTopicRecommendation(Section subTopic, CancellationToken cancellationToken)
    {
        IEnumerable<SubTopicRecommendation> subTopicRecommendations = await _repository.GetEntities<SubTopicRecommendation>(cancellationToken);

        return subTopicRecommendations.Where(subTopicRecommendation => subTopicRecommendation.Subtopic.Sys.Id.Equals(subTopic.Sys.Id)).First();
    }
}