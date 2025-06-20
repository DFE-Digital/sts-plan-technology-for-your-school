using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;

namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public abstract class CmsEntryDto
    {
        public CmsEntrySystemDetailsDto Sys { get; init; } = null!;
        public string Description { get; init; } = null!;
    }
}
