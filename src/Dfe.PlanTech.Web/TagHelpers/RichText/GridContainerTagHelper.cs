using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.RichText;

public class GridContainerTagHelper(
    ILoggerFactory loggerFactory,
    ICardContainerContentPartRenderer cardContentPartRenderer
) : TagHelper
{
    private readonly ILogger<GridContainerTagHelper> _logger = loggerFactory.CreateLogger<GridContainerTagHelper>();
    private readonly ICardContainerContentPartRenderer _cardContentPartRenderer = cardContentPartRenderer ?? throw new ArgumentNullException(nameof(cardContentPartRenderer));

    public IReadOnlyList<CmsComponentCardDto>? Content { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Content is null)
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
