using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class GridContainerRenderer : ICardContainerContentPartRenderer
{
    private readonly ICardContentPartRenderer _cardContentPartRenderer;
    public GridContainerRenderer(ICardContentPartRenderer cardContentPartRenderer)
    {
        _cardContentPartRenderer = cardContentPartRenderer;
    }
    public string ToHtml(IReadOnlyList<CsCard> content)
    {
        if (content.IsNullOrEmpty())
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
