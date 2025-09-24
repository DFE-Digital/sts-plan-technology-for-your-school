using System.Text;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

public class CardComponentRenderer : ICardContentPartRenderer
{
    public StringBuilder AddHtml(ComponentCardEntry? content, StringBuilder stringBuilder)
    {
        if (content is null)
        {
            return stringBuilder;
        }

        stringBuilder.Append("<div class=\"dfe-card\">");
        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"{content.Uri}\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">{content.InternalName}</a>");
        stringBuilder.Append("</h3>");
        stringBuilder.Append($"<p class=\"govuk-body-s\">{content.Description}</p>");
        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }
}
