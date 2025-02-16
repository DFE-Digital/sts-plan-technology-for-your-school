﻿using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.Components
{
    public class AccordionComponent : IRichTextContentPartRendererCollection
    {
        public ILogger Logger { get; }
        public IList<IRichTextContentPartRenderer> Renders { get; private set; }
        public AccordionComponent()
        {
        }

        public StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
        {
            Renders = rendererCollection.Renders;

            var nestedContent = content?.Data?.Target?.Content ?? null;
            if (nestedContent != null && nestedContent.Any())
            {
                foreach (var innerContent in nestedContent)
                {
                    RenderChildren(innerContent.RichText, stringBuilder);
                    return stringBuilder;

                }
            }
            return stringBuilder;
        }

        public void RenderChildren(RichTextContent content, StringBuilder stringBuilder)
        {
            foreach (var subContent in content.Content)
            {
                var renderer = GetRendererForContent(subContent);

                if (renderer == null)
                {
                    //_logger.LogWarning("Could not find renderer for {subContent}", subContent);
                    continue;
                }

                renderer.AddHtml(subContent, this, stringBuilder);
            }
        }

        public IRichTextContentPartRenderer? GetRendererForContent(RichTextContent content)
        => Renders.FirstOrDefault(renderer => renderer.Accepts(content));
    }
}
