using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped;

namespace Dfe.ContentSupport.Web.Services;

public class LayoutService : ILayoutService
{
    public CsPage GenerateLayout(CsPage page, HttpRequest request, string pageSlug)
    {
        if (!page.ShowVerticalNavigation)
            return page;

        return new CsPage
        {
            Heading = GetHeading(page, pageSlug),
            MenuItems = GenerateVerticalNavigation(page, request, pageSlug),
            Content = GetVisiblePageList(page, pageSlug),
            UpdatedAt = page.UpdatedAt,
            CreatedAt = page.CreatedAt,
            HasCitation = page.HasCitation,
            HasBackToTop = page.HasBackToTop,
            IsSitemap = page.IsSitemap,
            ShowVerticalNavigation = page.ShowVerticalNavigation,
            Slug = page.Slug
        };
    }


    public Heading GetHeading(CsPage page, string pageSlug)
    {
        var selectedPage = page.Content.Find(o => o.Slug == pageSlug);

        if (selectedPage is { UseParentHero: false })
            return new Heading
            {
                Title = selectedPage.Title ?? string.Empty,
                Subtitle = selectedPage.Subtitle ?? string.Empty
            };

        return page.Heading;
    }


    public List<PageLink> GenerateVerticalNavigation(CsPage page, HttpRequest request,
        string pageSlug)
    {
        var baseUrl = GetNavigationUrl(request);

        var menuItems = page.Content.Select(o => new PageLink
        {
            Title = o.Title ?? "",
            Subtitle = o.Subtitle ?? "",
            Url = $"{baseUrl}/{o.Slug}",
            IsActive = pageSlug == o.Slug
        }).ToList();

        if (string.IsNullOrEmpty(pageSlug) && menuItems.Count > 0)
            menuItems[0].IsActive = true;

        return menuItems;
    }


    public static List<CsContentItem> GetVisiblePageList(CsPage page, string pageSlug)
    {
        if (!string.IsNullOrEmpty(pageSlug))
            return page.Content.Where(o => o.Slug == pageSlug).ToList();


        return page.Content.GetRange(0, 1);
    }


    public string GetNavigationUrl(HttpRequest request)
    {
        var splitUrl = request.Path.ToString().Split("/");
        return string.Join("/", splitUrl.Take(3));
    }
}
