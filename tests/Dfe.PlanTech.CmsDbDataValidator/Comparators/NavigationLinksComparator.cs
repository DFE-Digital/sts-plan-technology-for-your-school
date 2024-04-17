using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class NavigationLinkComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["DisplayText", "Href", "OpenInNewTab"], "NavigationLink")
{
  public override Task ValidateContent()
  {
    ValidateNavigationLinks(_dbEntities.OfType<NavigationLinkDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateNavigationLinks(NavigationLinkDbEntity[] NavigationLinks)
  {
    foreach (var contentfulNavigationLink in _contentfulEntities)
    {
      ValidateNavigationLink(NavigationLinks, contentfulNavigationLink);
    }
  }

  private void ValidateNavigationLink(NavigationLinkDbEntity[] NavigationLinks, JsonNode contentfulNavigationLink)
  {
    var contentfulNavigationLinkId = contentfulNavigationLink.GetEntryId();

    if (contentfulNavigationLinkId == null)
    {
      Console.WriteLine($"Couldn't find ID for Contentful NavigationLink {contentfulNavigationLink}");
      return;
    }

    var matchingDbNavigationLink = NavigationLinks.FirstOrDefault(NavigationLink => NavigationLink.Id == contentfulNavigationLinkId);

    if (matchingDbNavigationLink == null)
    {
      Console.WriteLine($"No matching NavigationLink found for contentful NavigationLink with ID: {contentfulNavigationLinkId}");
      return;
    }

    ValidateProperties(contentfulNavigationLink, matchingDbNavigationLink);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.NavigationLink;
  }
}