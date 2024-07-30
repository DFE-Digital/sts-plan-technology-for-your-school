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

    private void ValidateNavigationLinks(NavigationLinkDbEntity[] navigationLinks)
    {
        foreach (var contentfulNavigationLink in _contentfulEntities)
        {
            ValidateNavigationLink(navigationLinks, contentfulNavigationLink);
        }
    }

    private void ValidateNavigationLink(NavigationLinkDbEntity[] navigationLinks, JsonNode contentfulNavigationLink)
    {
        var databaseNavLink = TryRetrieveMatchingDbEntity(navigationLinks, contentfulNavigationLink);
        if (databaseNavLink == null)
        {
            return;
        }

        ValidateProperties(contentfulNavigationLink, databaseNavLink);
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.NavigationLink;
    }
}
