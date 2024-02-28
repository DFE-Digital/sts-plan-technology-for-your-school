using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationFromContentfulQuery(IContentRepository repository) : IGetSubTopicRecommendation
{
    private readonly IContentRepository _repository = repository;
    
    public async Task<SubtopicRecommendation> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken = default)
    {

        var options = CreateGetEntityOptions(subTopicId);
        
        IEnumerable<SubtopicRecommendation> subTopicRecommendations = await _repository.GetEntities<SubtopicRecommendation>(options, cancellationToken);

        var subtopicRecommendation = subTopicRecommendations.FirstOrDefault() ?? throw new KeyNotFoundException($"Could not find subtopic recommendation for:  {subTopicId}");

        return subtopicRecommendation;
    }
    
    private GetEntitiesOptions CreateGetEntityOptions(string sectionId) =>
        new(4, new[] { new ContentQueryEquals() { Field = "fields.subtopic.en-US.sys.id", Value = sectionId } });
}