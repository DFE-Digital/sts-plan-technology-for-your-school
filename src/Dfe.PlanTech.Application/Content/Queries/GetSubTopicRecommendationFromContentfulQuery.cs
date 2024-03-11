using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationFromContentfulQuery(IContentRepository repository) : IGetSubTopicRecommendationQuery
{
    private readonly IContentRepository _repository = repository;

    public async Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken = default)
    {

        var options = CreateGetEntityOptions(subTopicId);

        IEnumerable<SubTopicRecommendation> subTopicRecommendations = await _repository.GetEntities<SubTopicRecommendation>(options, cancellationToken);

        var subtopicRecommendation = subTopicRecommendations.FirstOrDefault();

        return subtopicRecommendation;
    }

    private static GetEntitiesOptions CreateGetEntityOptions(string sectionId) =>
        new(4, [new ContentQueryEquals() { Field = "fields.subtopic.en-US.sys.id", Value = sectionId }]);
}