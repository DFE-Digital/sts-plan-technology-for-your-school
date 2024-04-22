using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class RecommendationIntrosComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Slug", "Maturity"], "RecommendationIntro")
{
  public override Task ValidateContent()
  {
    ValidateRecommendationIntros();
    return Task.CompletedTask;
  }

  public void ValidateRecommendationIntros()
  {
    foreach (var contentfulRecommendationIntro in _contentfulEntities)
    {
      ValidateRecommendationIntro(_dbEntities.OfType<RecommendationIntroDbEntity>().ToArray(), contentfulRecommendationIntro);
    }
  }

  private void ValidateRecommendationIntro(RecommendationIntroDbEntity[] databaseRecommendationIntros, JsonNode contentfulRecommendationIntro)
  {
    var databaseRecommendationIntro = ValidateChildEntityExistsInDb(databaseRecommendationIntros, contentfulRecommendationIntro);
    if (databaseRecommendationIntro == null)
    {
      return;
    }

    var headerValidationResult = ValidateChild<RecommendationIntroDbEntity>(databaseRecommendationIntro, "HeaderId", contentfulRecommendationIntro, "header");
    ValidateProperties(contentfulRecommendationIntro, databaseRecommendationIntro, headerValidationResult);
    ValidateChildren(contentfulRecommendationIntro, "content", databaseRecommendationIntro, dbRecommendationIntro => dbRecommendationIntro.Content);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.RecommendationIntros.Select(intro => new RecommendationIntroDbEntity()
    {
      Id = intro.Id,
      Slug = intro.Slug,
      Maturity = intro.Maturity,
      Content = intro.Content.Select(answer => new ContentComponentDbEntity()
      {
        Id = answer.Id
      }).ToList()
    });
  }
}