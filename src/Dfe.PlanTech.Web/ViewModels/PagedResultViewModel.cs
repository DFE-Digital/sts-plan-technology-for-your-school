namespace Dfe.PlanTech.Web.Models
{
    public class PagedResultViewModel<TEntity>
    {
        public List<TEntity> Items { get; set; } = [];
        public int Page { get; set; }
        public int Total { get; set; }
    }
}
