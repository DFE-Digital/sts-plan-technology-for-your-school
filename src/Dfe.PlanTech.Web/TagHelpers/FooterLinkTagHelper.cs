using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

/// <summary>
/// Renders a single navigation link in the footer.
/// </summary>
/// <remarks>Should be refactored in future to be any <see cref="NavigationLink"/>, and pass in HTML class used</remarks>
public class FooterLinkTagHelper(ILogger<FooterLinkTagHelper> logger) : TagHelper
{
    public INavigationLink? Link { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Link == null || !Link.IsValid || !TryBuildElement(output))
        {
            output.TagName = null;
            output.Content.SetHtmlContent("");
            logger.LogWarning("Missing or invalid {Name} {Link}", nameof(NavigationLink), Link);
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
            logger.LogError("No href or content to link to for {LinkType}", nameof(NavigationLink));
            return string.Empty;
        }

        return GetUrlForContent() ?? Link.Href ?? string.Empty;
    }

    private string? GetUrlForContent()
    {
        if (Link?.ContentToLinkTo == null)
        {
            return null;
        }

        if (Link.ContentToLinkTo is not IHasSlug hasSlug)
        {
            logger.LogError("Invalid content type received for Link. Expected {Interface} but type is {Concrete}", typeof(IHasSlug), Link.ContentToLinkTo.GetType());
            return null;
        }

        var firstCharacterIsSlash = hasSlug.Slug[0] == '/';

        var slug = firstCharacterIsSlash ? hasSlug.Slug.AsSpan(1) : hasSlug.Slug.AsSpan();

        return Link.ContentToLinkTo switch
        {
            IPage => $"/{slug}",
            IContentSupportPage => $"/content/{slug}",
            _ => LogInvalidContentTypeAndReturnNull(Link.ContentToLinkTo)
        };
    }



    private string? LogInvalidContentTypeAndReturnNull(object content)
    {
        logger.LogError("Unsupported content type {ContentType} in {TagHelper}",
            content.GetType().Name,
            nameof(FooterLinkTagHelper));
        return null;
    }
}
