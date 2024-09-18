using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
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
        var databaseRecommendationIntro = TryRetrieveMatchingDbEntity(databaseRecommendationIntros, contentfulRecommendationIntro);
        if (databaseRecommendationIntro == null)
        {
            return;
        }

        ValidateProperties(contentfulRecommendationIntro, databaseRecommendationIntro, GetValidationErrors(databaseRecommendationIntro, contentfulRecommendationIntro).ToArray());
    }

    protected IEnumerable<DataValidationError> GetValidationErrors(RecommendationIntroDbEntity databaseRecommendationIntro, JsonNode contentfulRecommendationIntro)
    {
        foreach (var child in ValidateChildren(contentfulRecommendationIntro, "content", databaseRecommendationIntro, dbRecommendationChunk => dbRecommendationChunk.Content))
        {
            yield return child;
        }
    }


    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.RecommendationIntros.Select(intro => new RecommendationIntroDbEntity()
        {
            Id = intro.Id,
            Slug = intro.Slug,
            Maturity = intro.Maturity,
            Header = intro.Header,
            Content = intro.Content.Select(answer => new ContentComponentDbEntity()
            {
                Id = answer.Id
            }).ToList()
        });
    }
}
