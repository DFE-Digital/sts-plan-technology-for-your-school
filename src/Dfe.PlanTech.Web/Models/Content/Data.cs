using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class Data
{
    public Target Target { get; set; } = null!;
    public Uri Uri { get; set; } = null!;
}
