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

    public async Task<SubtopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendation = await _repo.GetCompleteRecommendationsForSubtopic(subtopicId, cancellationToken);

            if (recommendation == null)
            {
                _logger.LogError("Unable to find recommendtion for {SubtopicId} in DB", subtopicId);
                return null;
            }

            var errors = ValidateRecommendationResponse(recommendation).ToArray();

            if (errors.Length > 0)
            {
                _logger.LogError("Recommendation for {SubtopicId} has several data issues. Returning null so that a retrieval from Contentful can occur. Errors:\n\n{Errors}", subtopicId, string.Join(Environment.NewLine, errors));
                return null;
            }

            return MapRecommendation(recommendation!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recommendation for {Subtopic} from DB", subtopicId);
            throw;
        }
    }

    public Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subtopicId, string maturity, CancellationToken cancellationToken = default)
    => _repo.GetRecommenationsViewDtoForSubtopicAndMaturity(subtopicId, maturity, cancellationToken);

    private SubtopicRecommendation MapRecommendation(SubtopicRecommendationDbEntity dbEntity)
    => _mapperConfiguration.Map<SubtopicRecommendationDbEntity, SubtopicRecommendation>(dbEntity);

    private static IEnumerable<string> ValidateRecommendationResponse(SubtopicRecommendationDbEntity recommendation)
    {
        if (recommendation.Intros == null || recommendation.Intros.Count == 0)
        {
            yield return "No intros found";
        }
    }
}