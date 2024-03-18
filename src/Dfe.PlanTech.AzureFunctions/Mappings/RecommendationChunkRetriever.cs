using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var recommendationChunk = await Db.RecommendationChunks.IgnoreQueryFilters()
                                                            .Select(chunk => new RecommendationChunkDbEntity()
                                                            {
                                                              Id = chunk.Id,
                                                              Title = chunk.Title,
                                                              HeaderId = chunk.HeaderId,
                                                              Deleted = chunk.Deleted,
                                                              Archived = chunk.Archived,
                                                              Published = chunk.Published,
                                                              Answers = chunk.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Id }).ToList(),
                                                              Content = chunk.Content.Select(content => new ContentComponentDbEntity() { Id = content.Id }).ToList(),
                                                            })
                                                            .FirstOrDefaultAsync(chunk => chunk.Id == entity.Id, cancellationToken);

    if (recommendationChunk == null)
    {
      return null;
    }

    return recommendationChunk;
  }
}