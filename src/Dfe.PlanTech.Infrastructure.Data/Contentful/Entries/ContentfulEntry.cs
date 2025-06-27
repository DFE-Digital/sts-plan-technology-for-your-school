using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public abstract class ContentfulEntry<TDto>
        where TDto : CmsEntryDto, new()
    {
        public ContentfulEntrySystemDetails Sys { get; init; } = null!;
        public string Description { get; init; } = null!;

        public TDto ToDto()
        {
            var dto = CreateDto();
            dto.Sys = Sys.ToDto();
            dto.Description = Description;

            return dto;
        }

        protected abstract TDto CreateDto();
    }
}
