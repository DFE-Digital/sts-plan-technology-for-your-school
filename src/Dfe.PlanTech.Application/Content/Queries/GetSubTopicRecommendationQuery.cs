using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendation([FromKeyedServices("Contentful")] IGetSubTopicRecommendationQuery getFromContentfulQuery, GetSubTopicRecommendationFromDbQuery getFromDbQuery) : IGetSubTopicRecommendationQuery
{
  private readonly IGetSubTopicRecommendationQuery _getFromContentfulQuery = getFromContentfulQuery;
  private readonly GetSubTopicRecommendationFromDbQuery _getFromDbQuery = getFromDbQuery;

  Task<SubTopicRecommendation?> IGetSubTopicRecommendationQuery.GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}