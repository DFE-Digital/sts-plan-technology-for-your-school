using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class RecommendationChunksComparatorclass(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Title"], "RecommendationChunk")
{
    public override Task ValidateContent()
    {
        ValidateRecommendationChunks();
        return Task.CompletedTask;
    }

    public void ValidateRecommendationChunks()
    {
        foreach (var contentfulRecommendationChunk in _contentfulEntities)
        {
            ValidateRecommendationChunk(_dbEntities.OfType<RecommendationChunkDbEntity>().ToArray(), contentfulRecommendationChunk);
        }
    }

    private void ValidateRecommendationChunk(RecommendationChunkDbEntity[] databaseRecommendationChunks, JsonNode contentfulRecommendationChunk)
    {
        var databaseRecommendationChunk = TryRetrieveMatchingDbEntity(databaseRecommendationChunks, contentfulRecommendationChunk);
        if (databaseRecommendationChunk == null)
        {
            return;
        }

        ValidateProperties(contentfulRecommendationChunk, databaseRecommendationChunk, GetValidationErrors(databaseRecommendationChunk, contentfulRecommendationChunk).ToArray());
    }

    protected IEnumerable<DataValidationError> GetValidationErrors(RecommendationChunkDbEntity databaseRecommendationChunk, JsonNode contentfulRecommendationChunk)
    {
        var headerValidationResult = ValidateChild<RecommendationChunkDbEntity>(databaseRecommendationChunk, "HeaderId", contentfulRecommendationChunk, "header");
        if (headerValidationResult != null)
        {
            yield return new DataValidationError("Header", headerValidationResult);
        }

        foreach (var child in ValidateChildren(contentfulRecommendationChunk, "answers", databaseRecommendationChunk, dbRecommendationChunk => dbRecommendationChunk.Answers))
        {
            yield return child;
        }

        foreach (var child in ValidateChildren(contentfulRecommendationChunk, "content", databaseRecommendationChunk, dbRecommendationChunk => dbRecommendationChunk.Content))
        {
            yield return child;
        }
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.RecommendationChunks.Select(chunk => new RecommendationChunkDbEntity()
        {
            Id = chunk.Id,
            HeaderId = chunk.HeaderId,
            Answers = chunk.Answers.Select(answer => new AnswerDbEntity()
            {
                Id = answer.Id
            }).ToList(),
            Content = chunk.Content.Select(content => new ContentComponentDbEntity()
            {
                Id = content.Id
            }).ToList(),
        });
    }
}