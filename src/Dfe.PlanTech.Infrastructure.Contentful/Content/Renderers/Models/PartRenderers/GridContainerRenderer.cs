using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class GridContainerRenderer : BaseRichTextContentPartRender, ICardContainerContentPartRenderer
{
    public GridContainerRenderer() : base(RichTextNodeType.GridContainer)
    {
        
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        throw new NotImplementedException();
    }

    public string ToHtml(List<CsCard> content)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"govuk-grid-row\">");
            stringBuilder.Append("<div class=\"govuk-grid-column-two-thirds\">");
        stringBuilder.Append("<div class=\"dfe-grid-container\">");
        foreach(var csCard in content)
        {
            stringBuilder.Append(AddCard(csCard));
        }
        stringBuilder.Append("</div></div></div>");

        return stringBuilder.ToString();
    }

    private StringBuilder AddCard(CsCard cardComponent)
    {
        var stringBuilder = new StringBuilder();
        if (cardComponent == null)
        {
            return null;
        }

        stringBuilder.Append("<div class=\"dfe-card\">");
        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"{cardComponent.Uri}\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">{cardComponent.InternalName}</a>");
        stringBuilder.Append("</h3>");
        stringBuilder.Append($"<p class=\"govuk-body-s\">{cardComponent.Description}</p>");
        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }
}
