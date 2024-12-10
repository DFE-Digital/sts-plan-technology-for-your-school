using System.Diagnostics.CodeAnalysis;
using Contentful.Core.Models;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class ContentItem : ContentItemBase
{
    public string Value { get; set; } = null!;
    public List<Mark> Marks { get; set; } = [];
    public Data Data { get; set; } = null!;
}
