using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubtopicRecommendationFromContentfulQuery(IContentRepository repository, ILogger<GetSubtopicRecommendationFromContentfulQuery> logger) : IGetSubTopicRecommendationQuery
{
    public const string ServiceKey = "Contentful";
    private readonly ILogger<GetSubtopicRecommendationFromContentfulQuery> _logger = logger;
    private readonly IContentRepository _repository = repository;

    public async Task<SubtopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default)
    {

        var options = CreateGetEntityOptions(subtopicId);

        IEnumerable<SubtopicRecommendation> subTopicRecommendations = await _repository.GetEntities<SubtopicRecommendation>(options, cancellationToken);

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

        var subtopicRecommendation = (await _repository.GetEntities<SubtopicRecommendation>(options, cancellationToken)).FirstOrDefault();

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
