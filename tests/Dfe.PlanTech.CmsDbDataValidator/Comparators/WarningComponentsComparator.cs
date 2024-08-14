using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class WarningComponentComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "WarningComponent")
{
    public override Task ValidateContent()
    {
        ValidateWarningComponents(_dbEntities.OfType<WarningComponentDbEntity>().ToArray());
        return Task.CompletedTask;
    }

    private void ValidateWarningComponents(WarningComponentDbEntity[] warningComponents)
    {
        foreach (var contentfulWarningComponent in _contentfulEntities)
        {
            ValidateWarningComponent(warningComponents, contentfulWarningComponent);
        }
    }

    private void ValidateWarningComponent(WarningComponentDbEntity[] warningComponents, JsonNode contentfulWarningComponent)
    {
        var matchingDbWarningComponent = TryRetrieveMatchingDbEntity(warningComponents, contentfulWarningComponent);

        if (matchingDbWarningComponent == null)
        {
            return;
        }

        var textValidationResult = ValidateChild<WarningComponentDbEntity>(matchingDbWarningComponent, "TextId", contentfulWarningComponent, "text");
        ValidateProperties(contentfulWarningComponent, matchingDbWarningComponent, TryGenerateDataValidationError("Text", textValidationResult));
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.Warnings;
    }
}
