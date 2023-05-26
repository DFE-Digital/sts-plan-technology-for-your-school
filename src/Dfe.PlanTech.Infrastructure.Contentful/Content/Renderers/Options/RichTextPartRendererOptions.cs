using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Options;

public class RichTextPartRendererOptions
{
    public string? Classes { get; init; }

    public void AddClasses(StringBuilder stringBuilder)
    {
        stringBuilder.Append(" class=\"");
        stringBuilder.Append(Classes);
    }
}
