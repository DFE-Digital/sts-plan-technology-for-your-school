using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class WarningEntry : ContentfulEntry
    {
        public TextBody Text { get; init; } = null!;
    }
}
