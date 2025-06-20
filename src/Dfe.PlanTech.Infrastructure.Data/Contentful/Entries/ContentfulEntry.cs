namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public abstract class ContentfulEntry
    {
        public ContentfulEntrySystemDetails Sys { get; init; } = null!;
        public string Description { get; init; } = null!;
    }
}
