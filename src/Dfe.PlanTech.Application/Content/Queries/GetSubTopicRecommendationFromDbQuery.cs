using AutoMapper;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationFromDbQuery(IRecommendationsRepository repo, ILogger<GetSubTopicRecommendationFromDbQuery> logger, IMapper mapperConfiguration) : IGetSubTopicRecommendationQuery
{
    public const string ServiceKey = "Database";

    private readonly ILogger<GetSubTopicRecommendationFromDbQuery> _logger = logger;
    private readonly IMapper _mapperConfiguration = mapperConfiguration;
    private readonly IRecommendationsRepository _repo = repo;

    public async Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendation = await _repo.GetCompleteRecommendationsForSubtopic(subtopicId, cancellationToken);

            if (recommendation == null)
            {
                return null;
            }

            return MapRecommendation(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recommendation for {Subtopic} from DB", subtopicId);
            throw;
        }
    }

    public Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subtopicId, string maturity, CancellationToken cancellationToken = default)
    => _repo.GetRecommenationsViewDtoForSubtopicAndMaturity(subtopicId, maturity, cancellationToken);

    private SubTopicRecommendation MapRecommendation(SubtopicRecommendationDbEntity dbEntity)
    => _mapperConfiguration.Map<SubtopicRecommendationDbEntity, SubTopicRecommendation>(dbEntity);
}