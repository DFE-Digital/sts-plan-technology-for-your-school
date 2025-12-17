using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class SystemDetails
{
    public string Id { get; set; } = null!;

    public SystemDetails() { }

    public SystemDetails(string id)
    {
        Id = id;
    }
}
