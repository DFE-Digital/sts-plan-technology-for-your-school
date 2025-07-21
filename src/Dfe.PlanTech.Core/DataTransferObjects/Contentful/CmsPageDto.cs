using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsPageDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public bool DisplayBackButton { get; set; }
        public bool DisplayHomeButton { get; set; }
        public bool DisplayTopicTitle { get; set; }
        public bool DisplayOrganisationName { get; set; }
        public bool RequiresAuthorisation { get; init; } = true;
        public List<CmsEntryDto> BeforeTitleContent { get; set; } = [];
        public CmsComponentTitleDto? Title { get; set; }
        public List<CmsEntryDto> Content { get; set; } = [];

        public CmsPageDto(PageEntry pageEntry)
        {
            InternalName = pageEntry.InternalName;
            Slug = pageEntry.Slug;
            DisplayBackButton = pageEntry.DisplayBackButton;
            DisplayHomeButton = pageEntry.DisplayHomeButton;
            DisplayTopicTitle = pageEntry.DisplayTopicTitle;
            DisplayOrganisationName = pageEntry.DisplayOrganisationName;
            RequiresAuthorisation = pageEntry.RequiresAuthorisation;
            BeforeTitleContent = pageEntry.BeforeTitleContent.Select(BuildBeforeTitleContentDto).ToList();
            Title = pageEntry.Title?.AsDto();
            Content = pageEntry.Content.Select(BuildContentDto).ToList();
        }

        private CmsEntryDto BuildBeforeTitleContentDto(ContentComponent contentComponent)
        {
            if (contentComponent is IDtoTransformable<CmsComponentWarningDto> warningEntry)
            {
                return warningEntry.AsDtoInternal();
            }

            if (contentComponent is IDtoTransformable<CmsComponentNotificationBannerDto> notificationEntry)
            {
                return notificationEntry.AsDtoInternal();
            }

            throw new ArgumentException($"{nameof(ContentComponent)} in {nameof(RecommendationIntroEntry)} was not of an expected type.");
        }
    }
}
