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
    var contentfulHeaderId = contentfulHeader.GetEntryId();

    if (contentfulHeaderId == null)
    {
      Console.WriteLine($"Couldn't find ID for Contentful header {contentfulHeader}");
      return;
    }

    var matchingDbHeader = headers.FirstOrDefault(header => header.Id == contentfulHeaderId);

    if (matchingDbHeader == null)
    {
      Console.WriteLine($"No matching header found for contentful header with ID: {contentfulHeaderId}");
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