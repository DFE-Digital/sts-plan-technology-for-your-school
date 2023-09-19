using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Dfe.PlanTech.Web.TagHelpers;

public class FooterLinkTagHelper : TagHelper
{
  private readonly ILogger<FooterLinkTagHelper> _logger;

  public NavigationLink? Link { get; set; }

  public FooterLinkTagHelper(ILogger<FooterLinkTagHelper> logger)
  {
    _logger = logger;
  }

  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    if (Link == null)
    {
      _logger.LogWarning($"Missing {nameof(Link)}");
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

  private void AppendCloseTag(StringBuilder stringBuilder)
  {
    stringBuilder.Append("</a>");
  }

  private void AppendOpenTag(StringBuilder stringBuilder)
  {
    stringBuilder.Append("<a class=\"govuk-footer__link\" href=\"");
    stringBuilder.Append(Link!.Href);
    stringBuilder.Append('"');

    if (Link.OpenInNewTab)
    {
      stringBuilder.Append(" target=\"_blank\"");
    }
    stringBuilder.Append('>');
  }
}
