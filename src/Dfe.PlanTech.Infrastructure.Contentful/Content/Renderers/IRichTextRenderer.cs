using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content;

public interface IRichTextRenderer
{    
    public string ToHtml(IRichTextContent content);
}
