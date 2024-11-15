using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class Fields
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public FileDetails File { get; set; } = null!;
}
