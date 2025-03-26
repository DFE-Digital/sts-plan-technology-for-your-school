using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class CardComponent : BaseRichTextContentPartRender
{
    public CardComponent() : base(RichTextNodeType.CsCard)
    {

    }

    private CustomCard GenerateCustomCard(RichTextContentData content)
    {
        return new CustomCard
        {
            InternalName = content?.InternalName ?? string.Empty,
            Description = content?.Asset.Description ?? string.Empty,
            //check this
            Meta = content?.Asset.Metadata.ToString(),
            Uri = content?.Asset.File.Url ?? string.Empty,
            ImageAlt = string.Empty,
        };
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var target = content?.Data?.Target;

        if (target == null)
        {
            return stringBuilder;
        }

        var cardComponent = GenerateCustomCard(target);

        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"{cardComponent.Uri}\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">\"{cardComponent.InternalName}\"</a>");
        stringBuilder.Append("</h3>");
        if (string.IsNullOrEmpty(cardComponent.Description))
        {
            stringBuilder.Append("<p> @Model.Description </p>");
        }
        if (string.IsNullOrEmpty(cardComponent.Meta))
        {
            stringBuilder.Append($"<p class=\"govuk-body-s>\"{cardComponent.Meta}\"</p>");
        }

        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }
}
