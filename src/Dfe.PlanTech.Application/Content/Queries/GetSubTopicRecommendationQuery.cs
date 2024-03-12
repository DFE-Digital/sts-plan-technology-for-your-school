using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationQuery([FromKeyedServices(GetSubTopicRecommendationFromContentfulQuery.ServiceKey)] IGetSubTopicRecommendationQuery getFromContentfulQuery,
                                            [FromKeyedServices(GetSubTopicRecommendationFromDbQuery.ServiceKey)] IGetSubTopicRecommendationQuery getFromDbQuery,
                                            ILogger<GetSubTopicRecommendationQuery> logger) : IGetSubTopicRecommendationQuery
{
  public const string ServiceKey = "Parent";

  private readonly IGetSubTopicRecommendationQuery _getFromContentfulQuery = getFromContentfulQuery;
  private readonly IGetSubTopicRecommendationQuery _getFromDbQuery = getFromDbQuery;
  private readonly ILogger<GetSubTopicRecommendationQuery> _logger = logger;

  public async Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken)
  {
    var fromDb = await _getFromDbQuery.GetSubTopicRecommendation(subTopicId, cancellationToken);

    if (fromDb != null)
    {
      LogRetrievalTrace(subTopicId, "database");
      return fromDb;
    }

    var fromContentful = await _getFromContentfulQuery.GetSubTopicRecommendation(subTopicId, cancellationToken);

    if (fromContentful != null)
    {
      LogRetrievalTrace(subTopicId, "Contentful");
      return fromContentful;
    }

    _logger.LogError("Was unable to find a subtopic recommendation for {SubtopicId} from DB or Contentful", subTopicId);
    return null;
  }

  private void LogRetrievalTrace(string subtopicId, string retrievedFrom)
  {
    _logger.LogTrace("Retrieved subtopic recommendation {SubtopicId} from {RetrievedFrom}", subtopicId, retrievedFrom);
  }
}