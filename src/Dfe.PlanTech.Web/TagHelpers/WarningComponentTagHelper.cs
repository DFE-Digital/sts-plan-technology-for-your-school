using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;
public class WarningComponentTagHelper : TagHelper
{
    public const string OpeningDiv = """<div class="govuk-warning-text">""";
    public const string WarningIcon = """<span class="govuk-warning-text__icon" aria-hidden="true">!</span>""";
    public const string OpeningSpan = """<strong class="govuk-warning-text__text">""";
    public const string AssistiveText = """<span class="govuk-visually-hidden">Warning</span>""";

    public WarningComponentTagHelper()
    {
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var inner = await output.GetChildContentAsync();
        var html = WarningComponentTagHelper.GetHtml(inner.GetContent());

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(html);
    }

    public static string GetHtml(string innerHtml)
    {
        var stringBuilder = new StringBuilder();
        AppendOpenTag(stringBuilder);

        stringBuilder.Append(innerHtml);

        AppendCloseTag(stringBuilder);

        return stringBuilder.ToString();
    }

    private static void AppendOpenTag(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(OpeningDiv);
        stringBuilder.AppendLine(WarningIcon);
        stringBuilder.AppendLine(OpeningSpan);
        stringBuilder.AppendLine(AssistiveText);
    }

    private static void AppendCloseTag(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("</strong>");
        stringBuilder.AppendLine("</div>");
    }
}
