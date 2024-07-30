using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class HeaderComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text"], "Header")
{
    public override Task ValidateContent()
    {
        ValidateHeaders(_dbEntities.OfType<HeaderDbEntity>().ToArray());
        return Task.CompletedTask;
    }

    private void ValidateHeaders(HeaderDbEntity[] headers)
    {
        foreach (var contentfulHeader in _contentfulEntities)
        {
            ValidateHeader(headers, contentfulHeader);
        }
    }

    private void ValidateHeader(HeaderDbEntity[] headers, JsonNode contentfulHeader)
    {
        var matchingDbHeader = TryRetrieveMatchingDbEntity(headers, contentfulHeader);

        if (matchingDbHeader == null)
        {
            return;
        }

        var tagValidationResult = ValidateEnumValue(contentfulHeader, "tag", matchingDbHeader, matchingDbHeader.Tag);
        var sizeValidationResult = ValidateEnumValue(contentfulHeader, "size", matchingDbHeader, matchingDbHeader.Size);

        ValidateProperties(contentfulHeader, matchingDbHeader, tagValidationResult, sizeValidationResult);
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.Headers;
    }
}
