using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class InsetTextComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text"], "InsetText")
{
    public override Task ValidateContent()
    {
        ValidateInsetTexts(_dbEntities.OfType<InsetTextDbEntity>().ToArray());
        return Task.CompletedTask;
    }

    private void ValidateInsetTexts(InsetTextDbEntity[] insetTexts)
    {
        foreach (var contentfulInsetText in _contentfulEntities)
        {
            ValidateInsetText(insetTexts, contentfulInsetText);
        }
    }

    private void ValidateInsetText(InsetTextDbEntity[] insetTexts, JsonNode contentfulInsetText)
    {
        var matchingDbInsetText = TryRetrieveMatchingDbEntity(insetTexts, contentfulInsetText);

        if (matchingDbInsetText == null)
        {
            return;
        }

        ValidateProperties(contentfulInsetText, matchingDbInsetText);
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.InsetTexts;
    }
}
