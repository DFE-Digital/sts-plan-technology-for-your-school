using System.Diagnostics.CodeAnalysis;
using Contentful.Core.Models;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class Target : Entry
{
    public new Fields Fields { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
    public string SummaryLine { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Meta { get; set; } = null!;
    public string ImageAlt { get; set; } = null!;
    public string Uri { get; set; } = null!;
    public Image Image { get; set; } = null!;
    public List<Target> Content { get; set; } = [];
    public new string Title { get; set; } = null!;
}
