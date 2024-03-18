using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var recommendationIntro = await Db.RecommendationIntros.IgnoreQueryFilters()
                                                          .Select(intro => new RecommendationIntroDbEntity()
                                                          {
                                                            Id = intro.Id,
                                                            Archived = intro.Archived,
                                                            Deleted = intro.Deleted,
                                                            Published = intro.Published,
                                                            Maturity = intro.Maturity,
                                                            Slug = intro.Slug,
                                                            HeaderId = intro.HeaderId,
                                                            Content = intro.Content.Select(c => new ContentComponentDbEntity() { Id = c.Id }).ToList()
                                                          })
                                                        .FirstOrDefaultAsync(intro => intro.Id == entity.Id, cancellationToken);

    if (recommendationIntro == null)
    {
      return null;
    }

    return recommendationIntro;
  }
}