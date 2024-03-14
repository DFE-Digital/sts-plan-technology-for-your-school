using AutoMapper;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationFromDbQuery(IRecommendationsRepository repo, ILogger<GetSubTopicRecommendationFromDbQuery> logger, IMapper mapperConfiguration) : IGetSubTopicRecommendationQuery
{
    public const string ServiceKey = "Database";

    private readonly ILogger<GetSubTopicRecommendationFromDbQuery> _logger = logger;
    private readonly IMapper _mapperConfiguration = mapperConfiguration;
    private readonly IRecommendationsRepository _repo = repo;

    public async Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken = default)
    {
        var dbEntity = await GetFromDb(subTopicId, cancellationToken);

        if (dbEntity == null)
        {
            return null;
        }

        return MapRecommendation(dbEntity);
    }

    private SubTopicRecommendation MapRecommendation(SubtopicRecommendationDbEntity dbEntity)
    => _mapperConfiguration.Map<SubtopicRecommendationDbEntity, SubTopicRecommendation>(dbEntity);

    private Task<SubtopicRecommendationDbEntity?> GetFromDb(string subtopicId, CancellationToken cancellationToken)
    {
        try
        {
            return _repo.GetRecommendationsForSubtopic(subtopicId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recommendation for {Subtopic} from DB", subtopicId);
            throw;
        }
    }
}