using System.Text;

namespace Dfe.PlanTech.Core.Contentful.Models.Options;

public class RichTextPartRendererOptions
{
    public string? Classes { get; init; }

    public void AddClasses(StringBuilder stringBuilder)
    {
        stringBuilder.Append(" class=\"");
        stringBuilder.Append(Classes);
        stringBuilder.Append('"');
    }
}
