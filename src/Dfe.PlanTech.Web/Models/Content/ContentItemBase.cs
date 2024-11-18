using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class ContentItemBase : ContentBase
{
    public string NodeType { get; set; } = null!;
    public Data Data { get; set; } = null!;
    public List<ContentItem> Content { get; set; } = [];
}
