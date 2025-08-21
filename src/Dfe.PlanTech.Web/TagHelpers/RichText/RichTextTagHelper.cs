using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.RichText;

public class RichTextTagHelper(
    ILoggerFactory loggerFactory,
    IRichTextRenderer richTextRenderer
) : TagHelper
{
    private readonly ILogger<RichTextTagHelper> _logger = loggerFactory.CreateLogger<RichTextTagHelper>();
    private readonly IRichTextRenderer _richTextRenderer = richTextRenderer ?? throw new ArgumentNullException(nameof(richTextRenderer));

    public RichTextContentField? Content { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Content == null)
        {
            _logger.LogWarning("Missing content");
            return;
        }

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;

        var html = _richTextRenderer.ToHtml(Content);
        output.Content.SetHtmlContent(html);
    }
}
