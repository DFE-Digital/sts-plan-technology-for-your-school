using Dfe.PlanTech.Core.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public abstract class ContentfulEntry
    {
        public ContentfulEntrySystemDetails Sys { get; init; } = null!;
        public string Description { get; init; } = null!;

        public CmsEntryDto ToDto()
        {
            var dto = CreateDto();
            dto.Sys = Sys.ToDto();
            dto.Description = Description;

            return dto;
        }

        protected abstract CmsEntryDto CreateDto();
    }
}
