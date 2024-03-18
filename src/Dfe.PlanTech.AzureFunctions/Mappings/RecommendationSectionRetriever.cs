using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationSectionRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var recommendationIntro = await Db.RecommendationSections.IgnoreQueryFilters()
                                                            .Select(section => new RecommendationSectionDbEntity()
                                                            {
                                                              Id = section.Id,
                                                              Deleted = section.Deleted,
                                                              Archived = section.Archived,
                                                              Published = section.Published,
                                                              Answers = section.Answers.Select(content => new AnswerDbEntity() { Id = content.Id }).ToList(),
                                                              Chunks = section.Chunks.Select(content => new RecommendationChunkDbEntity() { Id = content.Id }).ToList(),
                                                            })
                                                          .FirstOrDefaultAsync(section => section.Id == entity.Id, cancellationToken);

    if (recommendationIntro == null)
    {
      return null;
    }

    return recommendationIntro;
  }
}