using System.Text;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering
{
    public class AccordionComponentRenderer(ILogger<AccordionComponentRenderer> logger)
        : IRichTextContentPartRendererCollection
    {
        private readonly ILogger<AccordionComponentRenderer> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));

        public ILogger Logger => _logger;
        public IReadOnlyList<IRichTextContentPartRenderer> Renderers { get; private set; } = [];

        private const string _closingDiv = "</div>";

        public StringBuilder AddHtml(
            RichTextContentField content,
            IRichTextContentPartRendererCollection rendererCollection,
            StringBuilder stringBuilder
        )
        {
            Renderers = rendererCollection.Renderers;

            var nestedContent = content?.Data?.Target?.Content ?? null;
            if (nestedContent != null && nestedContent.Any())
            {
                var accordionId = $"accordion-{nestedContent[0].InternalName}";
                stringBuilder.AppendLine(
                    $"<div class=\"govuk-accordion\" data-module=\"govuk-accordion\" id=\"{accordionId}\">"
                );

                for (int i = 0; i < nestedContent.Count; i++)
                {
                    var innerContent = nestedContent[i];
                    var idx = i + 1;

                    var headingId = $"{accordionId}-heading-{idx}";
                    var contentId = $"{accordionId}-content-{idx}";
                    var summaryId = $"{accordionId}-summary-{idx}";

                    stringBuilder.Append("<div class=\"govuk-accordion__section\">");

                    stringBuilder.Append("<div class=\"govuk-accordion__section-header\">");
                    stringBuilder.Append("<h2 class=\"govuk-accordion__section-heading\">");

                    stringBuilder.Append(
                        $"<span class=\"govuk-accordion__section-button\" id=\"{headingId}\">"
                    );
                    stringBuilder.Append(innerContent.Title);
                    stringBuilder.Append("</span>");
                    stringBuilder.Append("</h2>");

                    if (!string.IsNullOrWhiteSpace(innerContent.SummaryLine))
                    {
                        stringBuilder.Append(
                            $"<div class=\"govuk-accordion__section-summary govuk-body\" id=\"{summaryId}\">"
                        );
                        stringBuilder.Append(innerContent.SummaryLine);
                        stringBuilder.Append(_closingDiv);
                    }

                    stringBuilder.Append(_closingDiv);

                    stringBuilder.Append(
                        $"<div id=\"{contentId}\" class=\"govuk-accordion__section-content\">"
                    );

                    if (innerContent.RichText != null)
                    {
                        RenderChildren(innerContent.RichText, stringBuilder);
                    }

                    stringBuilder.Append(_closingDiv);
                    stringBuilder.Append(_closingDiv);
                }

                stringBuilder.Append(_closingDiv);
            }
            return stringBuilder;
        }

        public void RenderChildren(RichTextContentField content, StringBuilder stringBuilder)
        {
            foreach (var subContent in content.Content)
            {
                var renderer = GetRendererForContent(subContent);

                if (renderer == null)
                {
                    Logger.LogWarning("Could not find renderer for {SubContent}", subContent);
                    continue;
                }

                renderer.AddHtml(subContent, this, stringBuilder);
            }
        }

        public IRichTextContentPartRenderer? GetRendererForContent(RichTextContentField content) =>
            Renderers.FirstOrDefault(renderer => renderer.Accepts(content));
    }
}
