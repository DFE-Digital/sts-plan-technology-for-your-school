using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.TagHelpers.RichText;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Dfe.PlanTech.Web.TagHelpers;
public class WarningComponentTagHelper : TagHelper
{
  private readonly ILogger<WarningComponentTagHelper> _logger;

  public WarningComponent? Warning { get; set; }

  public WarningComponentTagHelper(ILogger<WarningComponentTagHelper> logger)
  {
    _logger = logger;
  }

  public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
  {
    var inner = await output.GetChildContentAsync();
    var html = GetHtml(inner.GetContent());

    output.TagName = null;
    output.TagMode = TagMode.StartTagAndEndTag;
    output.Content.SetHtmlContent(html);
  }

  public string GetHtml(string innerHtml)
  {
    var stringBuilder = new StringBuilder();
    AppendOpenTag(stringBuilder);

    stringBuilder.Append(innerHtml);

    AppendCloseTag(stringBuilder);

    return stringBuilder.ToString();
  }

  private static void AppendCloseTag(StringBuilder stringBuilder)
  {
    stringBuilder.AppendLine("</strong>");
    stringBuilder.AppendLine("</div>");
  }

  private void AppendOpenTag(StringBuilder stringBuilder)
  {
    stringBuilder.AppendLine("""<div class="govuk-warning-text">""");
    stringBuilder.AppendLine("""<span class="govuk-warning-text__icon" aria-hidden="true">!</span>""");
    stringBuilder.AppendLine("""<strong class="govuk-warning-text__text">""");
    stringBuilder.AppendLine("""<span class="govuk-warning-text__assistive">Warning</span>""");
  }
}