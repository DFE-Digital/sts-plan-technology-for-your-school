using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

/// <summary>
/// Renders a single navigation link in the footer.
/// </summary>
/// <remarks>Should be refactored in future to be any <see cref="NavigationLink"/>, and pass in HTML class used</remarks>
public class FooterLinkTagHelper : TagHelper
{
    public const string FOOTER_CLASS = "\"govuk-footer__link\"";
    private readonly ILogger<FooterLinkTagHelper> _logger;

    public INavigationLink? Link { get; set; }

    public FooterLinkTagHelper(ILogger<FooterLinkTagHelper> logger)
    {
        _logger = logger;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Link == null || !Link.IsValid)
        {
            _logger.LogWarning("Missing {link}", nameof(Link));
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
        stringBuilder.Append("<a class=").Append(FOOTER_CLASS).Append(" href=\"");
        stringBuilder.Append(Link!.Href);
        stringBuilder.Append('"');

        if (Link.OpenInNewTab)
        {
            stringBuilder.Append(" target=\"_blank\"");
        }
        stringBuilder.Append('>');
    }
}
