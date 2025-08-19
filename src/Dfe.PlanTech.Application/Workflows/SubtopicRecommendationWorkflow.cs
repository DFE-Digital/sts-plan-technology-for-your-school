using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Content.Queries;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Workflows;

public class SubtopicRecommendationWorkflow(
    ILoggerFactory loggerFactory,
    IContentfulRepository contentfulRepository
)
{
    private readonly ILogger<SubtopicRecommendationWorkflow> _logger = loggerFactory.CreateLogger<SubtopicRecommendationWorkflow>();
    private readonly IContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));

    public async Task<SubtopicRecommendationEntry?> GetFirstSubtopicRecommendationAsync(string subtopicId)
    {
        var options = CreateGetEntityOptions(subtopicId, 4);
        var subtopicRecommendations = await _contentfulRepository.GetEntriesAsync<SubtopicRecommendationEntry>(options);

        var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();
        if (subtopicRecommendation is null)
        {
            LogMissingRecommendationError(subtopicId);
        }

        return subtopicRecommendation;
    }

    public async Task<RecommendationIntroEntry?> GetIntroForMaturityAsync(string subtopicId, string maturity)
    {
        var options = CreateGetEntityOptions(subtopicId, 2);
        options.Select = ["fields.intros", "sys"];

        var subtopicRecommendations = await _contentfulRepository.GetEntriesAsync<SubtopicRecommendationEntry>(options);

        var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();
        if (subtopicRecommendation is null)
        {
            LogMissingRecommendationError(subtopicId);
            return null;
        }

        var introForMaturity = subtopicRecommendation.GetRecommendationByMaturity(maturity);
        if (introForMaturity is null)
        {
            _logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
            return null;
        }

        return introForMaturity;
    }

    private static GetEntriesOptions CreateGetEntityOptions(string subtopicId, int depth = 4) =>
        new(depth, [new ContentfulQuerySingleValue() { Field = "fields.subtopic.sys.id", Value = subtopicId }]);

    private void LogMissingRecommendationError(string subtopicId) =>
        _logger.LogError("Could not find subtopic recommendation in Contentful for {SubtopicId}", subtopicId);
}

