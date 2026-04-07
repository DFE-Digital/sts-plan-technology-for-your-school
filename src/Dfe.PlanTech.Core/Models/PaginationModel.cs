using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class PaginationModel
{
    public int Total { get; set; }
    public int Page { get; set; }
}
