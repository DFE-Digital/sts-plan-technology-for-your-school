using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.RichText;

public class RichTextTagHelper : TagHelper
{
    private readonly IRichTextRenderer _richTextRenderer;

    public RichTextContent? Content { get; set; }

    public RichTextTagHelper(IRichTextRenderer richTextRenderer)
    {
        _richTextRenderer = richTextRenderer;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Content == null)
        {
            //TODO: Log missing rich text;
            return;
        }

        var html = _richTextRenderer.ToHtml(Content);
        output.Content.SetHtmlContent(html);
    }
}
