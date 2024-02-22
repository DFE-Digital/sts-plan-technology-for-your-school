using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

// TODO: Make interfaces
public class GetRecommendationChunksForSection(GetSubTopicRecommendationFromContentfulQuery getSubTopicRecommendationFromContentfulQuery)
{
    private readonly GetSubTopicRecommendationFromContentfulQuery _getSubTopicRecommendationFromContentfulQuery = getSubTopicRecommendationFromContentfulQuery;

    public async Task<IEnumerable<RecommendationChunk>> GetRecommendationChunksFromAnswers(Section subTopic, IEnumerable<Answer> answers, CancellationToken cancellationToken)
    {
        SubTopicRecommendation subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(subTopic, cancellationToken);

        RecommendationSection recommendationSection = subTopicRecommendation.Section;

        IEnumerable<RecommendationChunk> recommendationChunks = recommendationSection.Chunks;

        if (recommendationSection.Answers.IntersectBy(answers.Select(answer => answer.Sys.Id), recAnswer => recAnswer.Sys.Id).Any())
        {
            return recommendationChunks;
        }

        return recommendationChunks.Where(chunk => chunk.Answers.IntersectBy(answers.Select(answer => answer.Sys.Id), chunkAns => chunkAns.Sys.Id).Any());
    }
}