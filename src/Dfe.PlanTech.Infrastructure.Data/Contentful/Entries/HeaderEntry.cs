using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class HeaderEntry : ContentfulEntry
    {
        public string Text { get; init; } = null!;

        public HeaderTag Tag { get; init; }

        public HeaderSize Size { get; init; }
    }
}
