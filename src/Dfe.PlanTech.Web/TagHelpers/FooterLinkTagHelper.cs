using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

/// <summary>
/// Renders a single navigation link in the footer.
/// </summary>
/// <remarks>Should be refactored in future to be any <see cref="NavigationLink"/>, and pass in HTML class used</remarks>
public class FooterLinkTagHelper(
    ILoggerFactory loggerFactory
) : TagHelper
{
    public CmsNavigationLinkDto? Link { get; set; }

    private readonly ILogger<FooterLinkTagHelper> _logger = loggerFactory.CreateLogger<FooterLinkTagHelper>();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Link == null || !Link.IsValid || !TryBuildElement(output))
        {
            output.TagName = null;
            output.Content.SetHtmlContent("");
            _logger.LogWarning("Missing or invalid {Name} {Link}", nameof(CmsNavigationLinkDto), Link);
            return;
        }

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
    }

    /// <summary>
    /// Adds attributes to element
    /// </summary>
    /// <param name="output"></param>
    /// <returns>True; valid element. False; invalid</returns>
    public bool TryBuildElement(TagHelperOutput output)
    {
        var href = GetHref();

        if (string.IsNullOrEmpty(href))
        {
            return false;
        }

        output.Attributes.Add("href", href);
        output.Attributes.Add("class", "govuk-footer__link");

        if (Link!.OpenInNewTab)
        {
            output.Attributes.Add("target", "_blank");
        }

        return true;
    }

    private string GetHref()
    {
        if (Link!.ContentToLinkTo == null && string.IsNullOrEmpty(Link.Href))
        {
            _logger.LogError("No href or content to link to for {LinkType}", nameof(CmsNavigationLinkDto));
            return string.Empty;
        }

        return GetUrlForContent() ?? Link.Href ?? string.Empty;
    }

    private string? GetUrlForContent()
    {
        if (Link?.ContentToLinkTo is null)
        {
            return null;
        }

        if (Link.ContentToLinkTo is not IHasSlug hasSlug)
        {
            _logger.LogError("Invalid content type received for Link. Expected {Interface} but type is {Concrete}", typeof(IHasSlug), Link.ContentToLinkTo.GetType());
            return null;
        }

        var firstCharacterIsSlash = hasSlug.Slug[0] == '/';

        var slug = firstCharacterIsSlash ? hasSlug.Slug.AsSpan(1) : hasSlug.Slug.AsSpan();

        return $"/{slug}";
    }
}
