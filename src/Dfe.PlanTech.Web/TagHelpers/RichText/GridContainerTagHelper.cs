using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.RichText;

public class GridContainerTagHelper(
    ILogger<GridContainerTagHelper> logger,
    ICardContainerContentPartRenderer cardContentPartRenderer
) : TagHelper
{
    private readonly ICardContainerContentPartRenderer _cardContentPartRenderer = cardContentPartRenderer ?? throw new ArgumentNullException(nameof(cardContentPartRenderer));

    public IReadOnlyList<ComponentCardEntry>? Content { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Content is null)
        {
            logger.LogWarning("Missing content");
            return;
        }

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;

        var html = _cardContentPartRenderer.ToHtml(Content);
        output.Content.SetHtmlContent(html);
    }
}
