using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models.Content;
using Dfe.PlanTech.Web.Models.Content.Mapped;
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
        if (Link == null || !Link.IsValid)
        {
            logger.LogWarning("Missing {link}", nameof(Link));
            return;
        }

        var html = GetHtml();

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(html);
    }

    public string GetHtml()
    {
        var stringBuilder = new StringBuilder();
        AppendOpenTag(stringBuilder);
        stringBuilder.Append(Link!.DisplayText);
        AppendCloseTag(stringBuilder);

        return stringBuilder.ToString();
    }

    private static void AppendCloseTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append("</a>");
    }

    private void AppendOpenTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append("""<a class="govuk-footer__link" """).Append(" href=\"");
        stringBuilder.Append(GetHref());
        stringBuilder.Append('"');

        if (Link!.OpenInNewTab)
        {
            stringBuilder.Append(" target=\"_blank\"");
        }

        stringBuilder.Append('>');
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

        return Link.ContentToLinkTo switch
        {
            IPage page => $"/{page.Slug}",
            CsPage csPage  => $"/content/{csPage.Slug}", 
            ContentSupportPage contentSupportPage => $"/content/{contentSupportPage.Slug}", 
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
