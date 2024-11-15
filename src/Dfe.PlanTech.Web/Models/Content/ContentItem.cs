using System.Diagnostics.CodeAnalysis;
using Contentful.Core.Models;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class ContentItem : ContentItemBase
{
    public string Value { get; set; } = null!;
    public List<Mark> Marks { get; set; } = [];
}
