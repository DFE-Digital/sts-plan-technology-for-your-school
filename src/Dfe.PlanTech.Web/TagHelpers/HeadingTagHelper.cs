using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

public class HeaderTagHelper : TagHelper
{
    private readonly ILogger<HeaderTagHelper> _logger;

    public Header? Header { get; init; }

    public HeaderTagHelper(ILogger<HeaderTagHelper> logger)
    {
        _logger = logger;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Header == null)
        {
            _logger.LogWarning($"Missing {nameof(Header)}");
            return;
        }

        if (Header.Tag == Domain.Content.Enums.HeaderTag.Unknown)
        {
            _logger.LogWarning($"Could not find {nameof(Header.Tag)} for {nameof(Header)}");
        }

        var stringBuilder = new StringBuilder();
        AppendOpenTag(stringBuilder);
        stringBuilder.Append(Header.Text);
        AppendCloseTag(stringBuilder);

        output.Content.SetHtmlContent(stringBuilder.ToString());
    }

    private StringBuilder AppendCloseTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append("</");
        stringBuilder.Append(Header!.Tag.ToString());
        stringBuilder.Append('>');

        return stringBuilder;
    }

    private StringBuilder AppendOpenTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append('<');
        stringBuilder.Append(Header!.Tag.ToString());
        stringBuilder.Append(" class=\"");
        stringBuilder.Append(Header.GetClassForSize());
        stringBuilder.Append('>');

        return stringBuilder;
    }
}
