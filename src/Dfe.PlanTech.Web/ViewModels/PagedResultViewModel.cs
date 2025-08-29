namespace Dfe.PlanTech.Web.ViewModels;

public class PagedResultViewModel<TEntity>
{
    public List<TEntity> Items { get; set; } = [];
    public int Page { get; set; }
    public int Total { get; set; }
}
