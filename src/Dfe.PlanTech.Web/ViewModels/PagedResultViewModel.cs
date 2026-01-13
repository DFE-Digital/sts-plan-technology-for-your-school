using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class PagedResultViewModel<TEntity>
{
    public List<TEntity> Items { get; set; } = [];
    public int Page { get; set; }
    public int Total { get; set; }
}
