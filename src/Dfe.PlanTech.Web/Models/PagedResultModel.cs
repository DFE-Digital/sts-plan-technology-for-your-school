namespace Dfe.PlanTech.Web.Models
{
    public class PagedResultModel<TEntity>
    {
        public List<TEntity> Items { get; set; }
        public int Page { get; set; }
        public int Total { get; set; }
    }
}
