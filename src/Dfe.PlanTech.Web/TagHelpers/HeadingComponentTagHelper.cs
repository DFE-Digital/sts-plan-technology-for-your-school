using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

public class HeaderComponentTagHelper : TagHelper
{
    private readonly ILogger<HeaderComponentTagHelper> _logger;

    public Header? Model { get; set; }

    public HeaderComponentTagHelper(ILogger<HeaderComponentTagHelper> logger)
    {
        _logger = logger;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Model == null)
        {
            _logger.LogWarning($"Missing {nameof(Model)}");
            return;
        }

        if (Model.Tag == Domain.Content.Enums.HeaderTag.Unknown)
        {
            _logger.LogWarning($"Could not find {nameof(Model.Tag)} for {nameof(Model)}");
        }

        var html = GetHtml();

        output.Content.SetHtmlContent(html);
    }

    public string GetHtml()
    {
        var stringBuilder = new StringBuilder();
        AppendOpenTag(stringBuilder);
        stringBuilder.Append(Model!.Text);
        AppendCloseTag(stringBuilder);

        return stringBuilder.ToString();
    }

    private void AppendCloseTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append("</");
        stringBuilder.Append(Model!.Tag.ToString());
        stringBuilder.Append('>');
    }

    private void AppendOpenTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append('<');
        stringBuilder.Append(Model!.Tag.ToString());
        stringBuilder.Append(" class=\"");
        stringBuilder.Append(Model.GetClassForSize());
        stringBuilder.Append("\">");
    }
}
