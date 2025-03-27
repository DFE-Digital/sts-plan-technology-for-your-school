using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.RichText;

public class GridContainerTagHelper(ILogger<GridContainerTagHelper> logger, ICardContainerContentPartRenderer cardContentPartRenderer) : TagHelper
{
    private readonly ILogger<GridContainerTagHelper> _logger = logger;
    private readonly ICardContainerContentPartRenderer _cardContentPartRenderer = cardContentPartRenderer;

    public List<CsCard>? Content { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Content == null)
        {
            _logger.LogWarning("Missing content");
            return;
        }

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;

        var html = _cardContentPartRenderer.ToHtml(Content);
        output.Content.SetHtmlContent(html);
    }
}
