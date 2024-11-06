using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationQuery(IContentRepository repository,
                                            ILogger<GetSubTopicRecommendationQuery> logger,
                                            ICmsCache cache) : IGetSubTopicRecommendationQuery
{
    private readonly ILogger<GetSubTopicRecommendationQuery> _logger = logger;
    private readonly IContentRepository _repository = repository;
    public const string ServiceKey = "Contentful";

    public async Task<SubtopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(subtopicId);
        IEnumerable<SubtopicRecommendation> subTopicRecommendations = await cache.GetOrCreateAsync($"SubtopicRecommendation:{subtopicId}", () => _repository.GetEntities<SubtopicRecommendation>(options, cancellationToken));
        var subtopicRecommendation = subTopicRecommendations.FirstOrDefault();

        if (subtopicRecommendation == null)
        {
            LogMissingRecommendationError(subtopicId);
        }

        return subtopicRecommendation;
    }

    public async Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subtopicId, string maturity, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(subtopicId, 2);
        options.Select = ["fields.intros", "sys"];

        var subtopicRecommendation = (await cache.GetOrCreateAsync($"SubtopicRecommendation:{subtopicId}", () => _repository.GetEntities<SubtopicRecommendation>(options, cancellationToken))).FirstOrDefault();

        if (subtopicRecommendation == null)
        {
            LogMissingRecommendationError(subtopicId);
            return null;
        }

        var introForMaturity = subtopicRecommendation.GetRecommendationByMaturity(maturity);

        if (introForMaturity == null)
        {
            _logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
            return null;
        }

        return new RecommendationsViewDto(introForMaturity.Slug, introForMaturity.Header.Text);
    }

    private static GetEntitiesOptions CreateGetEntityOptions(string sectionId, int depth = 4, params IContentQuery[] additionalQueries) =>
        new(depth, [new ContentQueryEquals() { Field = "fields.subtopic.sys.id", Value = sectionId }, .. additionalQueries]);

    private void LogMissingRecommendationError(string subtopicId)
    => _logger.LogError("Could not find subtopic recommendation in Contentful for {SubtopicId}", subtopicId);
}
