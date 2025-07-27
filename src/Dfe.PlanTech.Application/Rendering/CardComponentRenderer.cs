using System.Text;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Application.Rendering;

public class CardComponentRenderer : ICardContentPartRenderer
{
    public StringBuilder AddHtml(CmsComponentCardDto? cardComponent, StringBuilder stringBuilder)
    {
        if (cardComponent is null)
        {
            return stringBuilder;
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
