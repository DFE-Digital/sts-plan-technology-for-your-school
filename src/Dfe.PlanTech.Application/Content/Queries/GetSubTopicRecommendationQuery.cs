using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetSubTopicRecommendationQuery(IContentRepository repository,
                                            ILogger<GetSubTopicRecommendationQuery> logger) : IGetSubTopicRecommendationQuery
{
    public async Task<SubtopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(subtopicId);
        var subTopicRecommendations = await repository.GetEntities<SubtopicRecommendation>(options, cancellationToken);

        var subtopicRecommendation = subTopicRecommendations.FirstOrDefault();

        if (subtopicRecommendation == null)
        {
            LogMissingRecommendationError(subtopicId);
        }
        else
        {
            subtopicRecommendation.Intros.ForEach(e => e.Header.OverrideHeaderParams(HeaderTag.H2, HeaderSize.Large));
        }

        return subtopicRecommendation;
    }

    public async Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subtopicId, string maturity, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(subtopicId, 2);
        options.Select = ["fields.intros", "sys"];

        var subtopicRecommendations = await repository.GetEntities<SubtopicRecommendation>(options, cancellationToken);

        var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();

        if (subtopicRecommendation == null)
        {
            LogMissingRecommendationError(subtopicId);
            return null;
        }

        var introForMaturity = subtopicRecommendation.GetRecommendationByMaturity(maturity);

        if (introForMaturity == null)
        {
            logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
            return null;
        }

        return new RecommendationsViewDto(introForMaturity.Slug, introForMaturity.Header.Text);
    }

    private static GetEntitiesOptions CreateGetEntityOptions(string sectionId, int depth = 4, params IContentQuery[] additionalQueries) =>
        new(depth, [new ContentQueryEquals() { Field = "fields.subtopic.sys.id", Value = sectionId }, .. additionalQueries]);

    private void LogMissingRecommendationError(string subtopicId)
    => logger.LogError("Could not find subtopic recommendation in Contentful for {SubtopicId}", subtopicId);
}
