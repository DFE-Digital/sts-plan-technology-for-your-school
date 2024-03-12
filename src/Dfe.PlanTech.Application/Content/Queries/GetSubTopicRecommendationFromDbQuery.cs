using AutoMapper;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationFromDbQuery(ICmsDbContext db, ILogger<GetSubTopicRecommendationFromDbQuery> logger, IMapper mapperConfiguration) : IGetSubTopicRecommendationQuery
{
    public const string ServiceKey = "Database";

    private readonly ICmsDbContext _db = db;
    private readonly ILogger<GetSubTopicRecommendationFromDbQuery> _logger = logger;
    private readonly IMapper _mapperConfiguration = mapperConfiguration;

    public async Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken = default)
    {
        var dbEntity = await GetFromDb(subTopicId, cancellationToken);

        if (dbEntity == null)
        {
            return null;
        }

        return MapRecommendation(dbEntity);
    }

    private SubTopicRecommendation MapRecommendation(SubTopicRecommendationDbEntity dbEntity)
    => _mapperConfiguration.Map<SubTopicRecommendationDbEntity, SubTopicRecommendation>(dbEntity);

    private Task<SubTopicRecommendationDbEntity?> GetFromDb(string subTopicId, CancellationToken cancellationToken)
    {
        try
        {
            var query = _db.SubtopicRecommendations.Where(subtopicRecommendation => subtopicRecommendation.SubtopicId == subTopicId);

            return _db.FirstOrDefaultAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recommendation for {Subtopic} from DB", subTopicId);
            throw;
        }
    }
}