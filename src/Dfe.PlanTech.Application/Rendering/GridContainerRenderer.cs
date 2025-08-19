using System.Text;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Application.Rendering;

public class GridContainerRenderer : ICardContainerContentPartRenderer
{
    private readonly ICardContentPartRenderer _cardContentPartRenderer;
    public GridContainerRenderer(ICardContentPartRenderer cardContentPartRenderer)
    {
        _cardContentPartRenderer = cardContentPartRenderer;
    }
    public string ToHtml(IReadOnlyList<ComponentCardEntry>? content)
    {
        if (content is null || !content.Any())
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<div class=\"govuk-grid-row\">");
        stringBuilder.Append("<div class=\"govuk-grid-column-two-thirds\">");
        stringBuilder.Append("<div class=\"dfe-grid-container\">");

        foreach (var csCard in content)
        {
            _cardContentPartRenderer.AddHtml(csCard, stringBuilder);
        }

        stringBuilder.Append("</div></div></div>");

        return stringBuilder.ToString();
    }
}
