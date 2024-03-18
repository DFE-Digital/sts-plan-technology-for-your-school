using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var recommendationIntro = await Db.RecommendationIntros.IgnoreQueryFilters()
                                                            .Include(intro => intro.Content)
                                                            .FirstOrDefaultAsync(intro => intro.Id == entity.Id, cancellationToken);

    if (recommendationIntro == null)
    {
      return null;
    }

    return recommendationIntro;
  }
}