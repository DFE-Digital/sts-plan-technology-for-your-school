using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SubtopicRecommendationRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var recommendationIntro = await Db.SubtopicRecommendations.IgnoreQueryFilters()
                            .Select(subtopicRecommendation => new SubtopicRecommendationDbEntity()
                            {
                              Deleted = subtopicRecommendation.Deleted,
                              Archived = subtopicRecommendation.Archived,
                              Published = subtopicRecommendation.Published,
                              SectionId = subtopicRecommendation.SectionId,
                              SubtopicId = subtopicRecommendation.SubtopicId,
                              Intros = subtopicRecommendation.Intros.Select(content => new RecommendationIntroDbEntity() { Id = content.Id }).ToList(),
                            })
                            .FirstOrDefaultAsync(page => page.Id == entity.Id, cancellationToken);

    if (recommendationIntro == null)
    {
      return null;
    }

    return recommendationIntro;
  }
}