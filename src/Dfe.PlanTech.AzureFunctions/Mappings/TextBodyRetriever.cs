using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class TextBodyRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var textBody = await Db.TextBodies.IgnoreAutoIncludes().FirstOrDefaultAsync(tb => tb.Id == entity.Id, cancellationToken);

    if (textBody == null)
    {
      return null;
    }

    return textBody;
  }
}