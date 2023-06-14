using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.RichText;

public class RichTextTagHelper : TagHelper
{
    private readonly ILogger<RichTextTagHelper> _logger;
    private readonly IRichTextRenderer _richTextRenderer;

    public RichTextContent? Content { get; set; }

    public RichTextTagHelper(ILogger<RichTextTagHelper> logger, IRichTextRenderer richTextRenderer)
    {
        _logger = logger;
        _richTextRenderer = richTextRenderer;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Content == null)
        {
            _logger.LogWarning("Missing content");
            return;
        }

        var html = _richTextRenderer.ToHtml(Content);
        output.Content.SetHtmlContent(html);
    }
}
