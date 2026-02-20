using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering.Markdown
{
    public interface IMarkdownRenderer
    {
        string Render(RichTextContentField textBody);
    }
}
