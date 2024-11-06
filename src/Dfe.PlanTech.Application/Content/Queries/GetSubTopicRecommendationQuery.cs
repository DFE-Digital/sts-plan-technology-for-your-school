using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationQuery([FromKeyedServices(GetSubtopicRecommendationFromContentfulQuery.ServiceKey)] IGetSubTopicRecommendationQuery getFromContentfulQuery,
                                            [FromKeyedServices(GetSubTopicRecommendationFromDbQuery.ServiceKey)] IGetSubTopicRecommendationQuery getFromDbQuery,
                                            ILogger<GetSubTopicRecommendationQuery> logger,
                                            IDistributedCache cache) : IGetSubTopicRecommendationQuery
{
    private readonly IGetSubTopicRecommendationQuery _getFromContentfulQuery = getFromContentfulQuery;
    private readonly IGetSubTopicRecommendationQuery _getFromDbQuery = getFromDbQuery;
    private readonly ILogger<GetSubTopicRecommendationQuery> _logger = logger;

    public async Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subtopicId, string maturity, CancellationToken cancellationToken = default)
    {
        Task<RecommendationsViewDto?> func(IGetSubTopicRecommendationQuery repository) => repository.GetRecommendationsViewDto(subtopicId, maturity, cancellationToken);

        var recommendationsView = await cache.GetOrCreateAsync($"RecommendationViewDto:{subtopicId}", () => GetFromDbOrContentfulIfNotFound(func, subtopicId));

        if (recommendationsView == null)
        {
            _logger.LogError("Was unable to find a subtopic recommendation for {SubtopicId} from DB or Contentful", subtopicId);
        }

        return recommendationsView;
    }

    public async Task<SubtopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default)
    {
        Task<SubtopicRecommendation?> func(IGetSubTopicRecommendationQuery repository) => repository.GetSubTopicRecommendation(subtopicId, cancellationToken);

        var recommendation = await cache.GetOrCreateAsync($"SubtopicRecommendation:{subtopicId}", () => GetFromDbOrContentfulIfNotFound(func, subtopicId));

        if (recommendation == null)
        {
            _logger.LogError("Was unable to find a subtopic recommendation for {SubtopicId} from DB or Contentful", subtopicId);
        }

        return recommendation;
    }

    private async Task<T?> GetFromDbOrContentfulIfNotFound<T>(Func<IGetSubTopicRecommendationQuery, Task<T?>> getFromInterface, string subtopicId)
      where T : class
    {
        var fromContentful = await getFromInterface(_getFromContentfulQuery);

        if (fromContentful != null)
        {
            LogRetrievalTrace(subtopicId, "Contentful");
            return fromContentful;
        }

        return default;
    }

    private void LogRetrievalTrace(string subtopicId, string retrievedFrom)
    {
        _logger.LogTrace("Retrieved subtopic recommendation {SubtopicId} from {RetrievedFrom}", subtopicId, retrievedFrom);
    }
}
