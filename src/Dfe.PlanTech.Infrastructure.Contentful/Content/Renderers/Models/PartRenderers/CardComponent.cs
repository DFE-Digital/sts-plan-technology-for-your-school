using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class CardComponent : ICardContentPartRenderer
{
    public CardComponent()
    {
    }

    public StringBuilder AddHtml(CsCard? cardComponent, StringBuilder stringBuilder)
    {
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
